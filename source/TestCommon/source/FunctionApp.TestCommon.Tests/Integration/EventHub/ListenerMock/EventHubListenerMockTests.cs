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

using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.EventHub.ListenerMock;

public class EventHubListenerMockTests
{
    [Collection(nameof(EventHubListenerMockCollectionFixture))]
    public class InitializeAsync : EventHubListenerMockTestsBase
    {
        public InitializeAsync(EventHubListenerMockFixture listenerMockFixture)
            : base(listenerMockFixture)
        {
        }

        [Fact]
        public async Task When_EventHubExists_Then_EventsAreReceived()
        {
            // Arrange
            var eventHub = await ResourceProvider
                .BuildEventHub("evh-01")
                .CreateAsync();

            Sut = CreateSut(eventHub.Name, "container-01");

            // Act
            await Sut.InitializeAsync();

            // Assert
            var numberOfEvents = 3;
            using var eventBatch = await CreateEventBatchAsync(eventHub.ProducerClient, numberOfEvents);
            await eventHub.ProducerClient.SendAsync(eventBatch);

            using var countDownEvent = new CountdownEvent(numberOfEvents);
            await Sut.AddEventHandlerAsync(
                _ =>
                {
                    return true;
                },
                _ =>
                {
                    countDownEvent.Signal();
                    return Task.CompletedTask;
                });

            var allWasReceived = countDownEvent.Wait(DefaultTimeout);
            allWasReceived.Should().BeTrue();

            Sut.ReceivedEvents.Count.Should().Be(numberOfEvents);
        }

        [Fact]
        public async Task When_EventHubNameDoesNotExist_Then_InvalidOperationExceptionIsThrown()
        {
            // Arrange
            Sut = CreateSut("evh-02", "container-02");

            // Act
            Func<Task> act = Sut.InitializeAsync;

            // Assert
            await act.Should()
                .ThrowAsync<EventHubsException>()
                .WithMessage($"*{Sut.EventHubName}' could not be found*");
        }
    }

    [Collection(nameof(EventHubListenerMockCollectionFixture))]
    public class Reset : EventHubListenerMockTestsBase
    {
        public Reset(EventHubListenerMockFixture listenerMockFixture)
            : base(listenerMockFixture)
        {
        }

        [Fact]
        public async Task When_PreviouslyUsed_Then_InstanceCanBeReused()
        {
            // Arrange
            var eventHub = await ResourceProvider
                .BuildEventHub("evh")
                .CreateAsync();

            Sut = CreateSut(eventHub.Name, "container");

            await Sut.InitializeAsync();
            await AssertOneEventIsSendAndReceivedAsync(Sut, eventHub.ProducerClient);

            // Act
            Sut.Reset();

            // Assert
            await AssertOneEventIsSendAndReceivedAsync(Sut, eventHub.ProducerClient);
        }

        private async Task AssertOneEventIsSendAndReceivedAsync(EventHubListenerMock sut, EventHubProducerClient producerClient)
        {
            using var eventBatch = await CreateEventBatchAsync(producerClient, numberOfEvents: 1);
            await producerClient.SendAsync(eventBatch);

            using var resetEvent = new ManualResetEventSlim(false);
            await sut.AddEventHandlerAsync(
                _ =>
                {
                    return true;
                },
                _ =>
                {
                    resetEvent.Set();
                    return Task.CompletedTask;
                });

            var isReceived = resetEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();

            sut.ReceivedEvents.Count.Should().Be(1);
        }
    }

    /// <summary>
    /// Test <see cref="WhenProvider.When(EventHubListenerMock, Func{EventData, bool})"/>
    /// and <see cref="DoProvider.DoAsync"/>, including related extensions.
    /// </summary>
    [Collection(nameof(EventHubListenerMockCollectionFixture))]
    public class WhenDoProviders : EventHubListenerMockTestsBase
    {
        /// <summary>
        /// Tests depends on the fact that an event hub has been added in <see cref="OnInitializeAsync"/> and the listener has been initialized.
        /// </summary>
        public WhenDoProviders(EventHubListenerMockFixture listenerMockFixture)
            : base(listenerMockFixture)
        {
        }

        [NotNull]
        private EventHubResource? EventHub { get; set; }

        [Fact]
        public async Task When_EventMatch_Then_DoIsTriggered()
        {
            // Arrange
            var messageId = Guid.NewGuid().ToString();
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient, messageId);
            using var isReceivedEvent = new ManualResetEventSlim(false);

            await Sut
                .When(receivedEvent =>
                    receivedEvent.MessageId == messageId)
                .DoAsync(_ =>
                {
                    isReceivedEvent.Set();
                    return Task.CompletedTask;
                });

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();
        }

        [Fact]
        public async Task When_AnyEventAlreadyReceived_Then_DoIsTriggered()
        {
            // Arrange
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient);
            await EventHub.ProducerClient.SendAsync(eventBatch);
            await Awaiter.WaitUntilConditionAsync(() => Sut.ReceivedEvents.Count == 1, DefaultTimeout);

            // Act
            using var isReceivedEvent = await Sut
                .WhenAny()
                .VerifyOnceAsync();

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();
        }

        [Fact]
        public async Task When_OneEventAlreadyReceivedAndSecondEventIsSentAfterSettingUpHandler_Then_DoIsTriggered()
        {
            // Arrange
            var messageId1 = Guid.NewGuid().ToString();
            using var eventBatch1 = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient, messageId1);
            await EventHub.ProducerClient.SendAsync(eventBatch1);
            await Awaiter.WaitUntilConditionAsync(() => Sut.ReceivedEvents.Count == 1, DefaultTimeout);

            var eventsReceivedInHandler = new List<EventData>();
            using var isReceivedEvent = await Sut
                .WhenAny()
                .VerifyCountAsync(
                    2,
                    receivedEvent =>
                    {
                        eventsReceivedInHandler.Add(receivedEvent);
                        return Task.CompletedTask;
                    });

            var messageId2 = Guid.NewGuid().ToString();
            using var eventBatch2 = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient, messageId2);

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch2);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();

            eventsReceivedInHandler.Should()
                .Contain(receivedEvent => receivedEvent.MessageId.Equals(messageId1))
                .And.Contain(receivedEvent => receivedEvent.MessageId.Equals(messageId2));
        }

        [Fact]
        public async Task When_AnyEvent_Then_DoIsTriggered()
        {
            // Arrange
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient);
            using var isReceivedEvent = new ManualResetEventSlim(false);

            await Sut.WhenAny()
                .DoAsync(_ =>
                {
                    isReceivedEvent.Set();
                    return Task.CompletedTask;
                });

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();
        }

        [Fact]
        public async Task When_AnyEvent_Then_VerifyOnce()
        {
            // Arrange
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient);

            using var isReceivedEvent = await Sut
                .WhenAny()
                .VerifyOnceAsync();

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().BeTrue();
        }

        [Theory]
        [InlineData("123", "123", true)]
        [InlineData("123", "456", false)]
        public async Task When_MessageIdFilter_Then_VerifyOnceIfMatch(string messageId, string matchMessageId, bool expectDoIsTriggered)
        {
            // Arrange
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient, messageId);

            using var isReceivedEvent = await Sut
                .WhenMessageId(matchMessageId)
                .VerifyOnceAsync();

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().Be(expectDoIsTriggered);
        }

        [Theory]
        [InlineData("123", "123", true)]
        [InlineData("123", "456", false)]
        public async Task When_MessageIdFilterAndEventIsReplayed_Then_VerifyOnceIfMatch(string messageId, string matchMessageId, bool expectDoIsTriggered)
        {
            // Arrange
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient, messageId);

            await EventHub.ProducerClient.SendAsync(eventBatch);
            await Awaiter.WaitUntilConditionAsync(() => Sut.ReceivedEvents.Count == 1, DefaultTimeout);

            // Act
            using var isReceivedEvent = await Sut
                .WhenMessageId(matchMessageId)
                .VerifyOnceAsync();

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().Be(expectDoIsTriggered);
        }

        [Theory]
        [InlineData("123", "123", true)]
        [InlineData("123", "456", false)]
        public async Task When_CorrelationIdFilter_Then_VerifyOnceIfMatch(string correlationId, string matchCorrelationId, bool expectDoIsTriggered)
        {
            // Arrange
            using var eventBatch = await CreateEventBatchWithIdsAsync(EventHub.ProducerClient, correlationId: correlationId);

            using var isReceivedEvent = await Sut
                .WhenCorrelationId(matchCorrelationId)
                .VerifyOnceAsync();

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch);

            // Assert
            var isReceived = isReceivedEvent.Wait(DefaultTimeout);
            isReceived.Should().Be(expectDoIsTriggered);
        }

        [Fact]
        public async Task When_AnyEvent_Then_VerifyCount()
        {
            // Arrange
            var expectedCount = 3;
            using var whenAllEvent = await Sut
                .WhenAny()
                .VerifyCountAsync(expectedCount);

            using var eventBatch = await CreateEventBatchAsync(EventHub.ProducerClient, expectedCount);

            // Act
            await EventHub.ProducerClient.SendAsync(eventBatch);

            // Assert
            var allReceived = whenAllEvent.Wait(DefaultTimeout);
            allReceived.Should().BeTrue();
        }

        /// <summary>
        /// Preparing all <see cref="WhenDoProviders"/> tests with a event hub and an initialized listener.
        /// </summary>
        protected override async Task OnInitializeAsync()
        {
            EventHub = await ResourceProvider
                .BuildEventHub("evh")
                .CreateAsync();

            Sut = CreateSut(EventHub.Name, "container");
            await Sut.InitializeAsync();
        }
    }

    /// <summary>
    /// A new <see cref="EventHubListenerMock"/> is created and disposed for each test.
    /// Similar we create a new <see cref="EventHubResourceProvider"/> for each test,
    /// so we can delete any created resources (Event Hubs) between tests.
    /// </summary>
    public abstract class EventHubListenerMockTestsBase : IAsyncLifetime
    {
        protected EventHubListenerMockTestsBase(EventHubListenerMockFixture listenerMockFixture)
        {
            ListenerMockFixture = listenerMockFixture;
            ResourceProvider = new EventHubResourceProvider(
                ListenerMockFixture.TestLogger,
                ListenerMockFixture.NamespaceName,
                ListenerMockFixture.ResourceManagementSettings,
                ListenerMockFixture.Credential);
        }

        [NotNull]
        protected EventHubListenerMock? Sut { get; set; }

        protected EventHubListenerMockFixture ListenerMockFixture { get; }

        protected EventHubResourceProvider ResourceProvider { get; }

        protected TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

        public Task InitializeAsync()
        {
            return OnInitializeAsync();
        }

        public async Task DisposeAsync()
        {
            if (Sut != null)
            {
                await Sut.DisposeAsync();
            }

            await ResourceProvider.DisposeAsync();
        }

        protected EventHubListenerMock CreateSut(string eventHubName, string blobContainerName)
        {
            return new EventHubListenerMock(
                ListenerMockFixture.TestLogger,
                ListenerMockFixture.FullyQualifiedNamespace,
                eventHubName,
                ListenerMockFixture.BlobStorageServiceUri,
                blobContainerName,
                ListenerMockFixture.Credential);
        }

        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        protected static async Task<EventDataBatch> CreateEventBatchAsync(EventHubProducerClient producerClient, int numberOfEvents = 3)
        {
            var eventBatch = await producerClient.CreateBatchAsync();

            for (var i = 1; i <= numberOfEvents; i++)
            {
                if (!eventBatch.TryAdd(new EventData($"Event {i}")))
                {
                    // If it is too large for the batch
                    throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                }
            }

            return eventBatch;
        }

        protected static async Task<EventDataBatch> CreateEventBatchWithIdsAsync(EventHubProducerClient producerClient, string? messageId = null, string? correlationId = null)
        {
            var eventData = new EventData($"Event {messageId}")
            {
                MessageId = messageId,
                CorrelationId = correlationId,
            };

            var eventBatch = await producerClient.CreateBatchAsync();
            if (!eventBatch.TryAdd(eventData))
            {
                // If it is too large for the batch
                throw new Exception($"Event is too large for the batch and cannot be sent.");
            }

            return eventBatch;
        }
    }
}
