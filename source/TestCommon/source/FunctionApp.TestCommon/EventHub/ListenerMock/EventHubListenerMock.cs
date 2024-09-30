// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Concurrent;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.TestCommon.Diagnostics;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;

/// <summary>
/// Simple Event Hub listener mock with fluent API for setup.
///
/// Can listen for events on an event hub.
///
/// Reads any events instantly from its source, and keeps
/// an in memory log of the events received.
/// </summary>
public sealed class EventHubListenerMock : IAsyncDisposable
{
    public const string DefaultConsumerGroupName = "$Default";

    public EventHubListenerMock(
        ITestDiagnosticsLogger testLogger,
        string eventHubFullyQualifiedNamespace,
        string eventHubName,
        Uri blobStorageServiceUri,
        string blobContainerName,
        TokenCredential credential)
    {
        TestLogger = testLogger
            ?? throw new ArgumentNullException(nameof(testLogger));
        EventHubFullyQualifiedNamespace = string.IsNullOrWhiteSpace(eventHubFullyQualifiedNamespace)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventHubFullyQualifiedNamespace))
            : eventHubFullyQualifiedNamespace;
        EventHubName = string.IsNullOrWhiteSpace(eventHubName)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventHubName))
            : eventHubName;
        BlobContainerUri = BuildBlobContainerUri(blobStorageServiceUri, blobContainerName);
        ArgumentNullException.ThrowIfNull(credential);

        StorageClient = new BlobContainerClient(BlobContainerUri, credential);
        ProcessorClient = new EventProcessorClient(
            StorageClient,
            DefaultConsumerGroupName,
            EventHubFullyQualifiedNamespace,
            EventHubName,
            credential);

        EventHandlers = new ConcurrentDictionary<Func<EventData, bool>, Func<EventData, Task>>();

        MutableReceivedEventsLock = new SemaphoreSlim(1, 1);
        var mutableReceivedEvents = new BlockingCollection<EventData>();
        MutableReceivedEvents = mutableReceivedEvents;
        ReceivedEvents = mutableReceivedEvents;
    }

    /// <summary>
    /// The fully qualified namespace of the Event Hub Namespace under which the Event Hub exists.
    /// </summary>
    public string EventHubFullyQualifiedNamespace { get; }

    /// <summary>
    /// The name of the Event Hub for which the processor is created.
    /// </summary>
    public string EventHubName { get; }

    /// <summary>
    /// Uri referencing the blob container used for checkpointing.
    /// It includes the name of the account and the name of the container. This is likely to be similar to
    /// "https://{account_name}.blob.core.windows.net/{container_name}".
    /// For checkpointing, see: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-features#checkpointing
    /// </summary>
    public Uri BlobContainerUri { get; }

    /// <summary>
    /// Collection of all received events.
    /// </summary>
    public IReadOnlyCollection<EventData> ReceivedEvents { get; private set; }

    private ITestDiagnosticsLogger TestLogger { get; }

    private BlobContainerClient StorageClient { get; }

    private EventProcessorClient ProcessorClient { get; }

    private IDictionary<Func<EventData, bool>, Func<EventData, Task>> EventHandlers { get; }

    private SemaphoreSlim MutableReceivedEventsLock { get; }

    private BlockingCollection<EventData> MutableReceivedEvents { get; set; }

    public async Task InitializeAsync()
    {
        if (ProcessorClient.IsRunning)
        {
            throw new InvalidOperationException("Processor is already running.");
        }

        await StorageClient.CreateIfNotExistsAsync()
            .ConfigureAwait(false);

        ProcessorClient.ProcessEventAsync += HandleEventReceivedAsync;
        ProcessorClient.ProcessErrorAsync += HandleEventPumpExceptionAsync;

        await ProcessorClient.StartProcessingAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Close listener and dispose resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Add an event handler <paramref name="eventHandler"/> that will be used if event data matches <paramref name="eventMatcher"/>.
    /// If multiple event handlers can be matched with an event, then only one will be used (no guarantee for which one).
    /// NOTE: The handler supplied will be invoked for all events already received and currently present in the ReceivedEvents collection.
    /// </summary>
    public async Task AddEventHandlerAsync(Func<EventData, bool> eventMatcher, Func<EventData, Task> eventHandler)
    {
        var alreadyReceivedEvents = new List<EventData>();
        await MutableReceivedEventsLock.WaitAsync()
            .ConfigureAwait(false);

        try
        {
            EventHandlers.Add(eventMatcher, eventHandler);
            alreadyReceivedEvents.AddRange(ReceivedEvents);
        }
        finally
        {
            MutableReceivedEventsLock.Release();
        }

        // Replay already received events on eventHandler
        foreach (var alreadyReceivedEvent in alreadyReceivedEvents)
        {
            if (eventMatcher(alreadyReceivedEvent))
            {
                await eventHandler(alreadyReceivedEvent)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Reset handlers and received events.
    /// </summary>
    /// <remarks>Use this between tests.</remarks>
    public void Reset()
    {
        EventHandlers.Clear();
        ClearReceivedEvents();
    }

    private static Uri BuildBlobContainerUri(Uri blobStorageServiceUri, string blobContainerName)
    {
        ArgumentNullException.ThrowIfNull(blobContainerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobContainerName);

        return new Uri($"{blobStorageServiceUri.AbsoluteUri}/{blobContainerName}");
    }

    private static bool IsDefaultEventHandler(KeyValuePair<Func<EventData, bool>, Func<EventData, Task>> eventHandler)
    {
        return eventHandler.Equals(default(KeyValuePair<Func<EventData, bool>, Func<EventData, Task>>));
    }

    private static Func<EventData, Task> DefaultEventHandler()
    {
        return eventData => Task.CompletedTask;
    }

    private async Task HandleEventReceivedAsync(ProcessEventArgs eventArgs)
    {
        try
        {
            // If the cancellation token is signaled, then the processor has been asked to stop.
            // It will invoke this handler with any events that were in flight; these will not be
            // lost if not processed.
            if (eventArgs.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (eventArgs.HasEvent)
            {
                Func<EventData, Task> eventHandler;
                await MutableReceivedEventsLock.WaitAsync(eventArgs.CancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    MutableReceivedEvents.Add(eventArgs.Data, eventArgs.CancellationToken);
                    eventHandler = GetEventHandler(eventArgs.Data);
                }
                finally
                {
                    MutableReceivedEventsLock.Release();
                }

                try
                {
                    await eventHandler(eventArgs.Data)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Catch the exception so the event is "handled".
                    TestLogger.WriteLine($"{nameof(EventHubListenerMock)}: {ex}");
                }

                await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            TestLogger.WriteLine($"{nameof(EventHubListenerMock)}: {ex}");
        }
    }

    private Func<EventData, Task> GetEventHandler(EventData eventData)
    {
        var eventHandler = EventHandlers.FirstOrDefault(x => x.Key(eventData));
        return IsDefaultEventHandler(eventHandler)
            ? DefaultEventHandler()
            : eventHandler.Value;
    }

    /// <summary>
    /// If the underlying event pump throws an exception it will arrive here.
    /// </summary>
    private Task HandleEventPumpExceptionAsync(ProcessErrorEventArgs eventArgs)
    {
        TestLogger.WriteLine($"{nameof(EventHubListenerMock)}: {eventArgs.Exception}");
        return Task.CompletedTask;
    }

    private async ValueTask DisposeAsyncCore()
    {
        await ProcessorClient.StopProcessingAsync()
            .ConfigureAwait(false);

        ProcessorClient.ProcessEventAsync -= HandleEventReceivedAsync;
        ProcessorClient.ProcessErrorAsync -= HandleEventPumpExceptionAsync;

        MutableReceivedEvents.CompleteAdding();
        MutableReceivedEvents.Dispose();

        MutableReceivedEventsLock.Dispose();

        await StorageClient.DeleteIfExistsAsync()
            .ConfigureAwait(false);
    }

    private void ClearReceivedEvents()
    {
        MutableReceivedEvents.CompleteAdding();
        MutableReceivedEvents.Dispose();

        // As soon as we have called "CompleteAdding" we must create a new instance.
        var mutableReceivedEvents = new BlockingCollection<EventData>();
        MutableReceivedEvents = mutableReceivedEvents;
        ReceivedEvents = mutableReceivedEvents;
    }
}
