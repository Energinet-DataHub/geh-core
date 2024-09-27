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

using Azure.ResourceManager.EventHubs;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;

/// <summary>
/// Fluent API for creating an event hub resource.
/// </summary>
public class EventHubResourceBuilder : IEventHubResourceBuilder
{
    internal EventHubResourceBuilder(EventHubResourceProvider resourceProvider, string eventHubName, EventHubData createEventHubOptions)
    {
        ResourceProvider = resourceProvider;
        EventHubName = eventHubName;
        CreateEventHubOptions = createEventHubOptions;
        ConsumerGroupBuilders = new Dictionary<string, EventHubConsumerGroupBuilder>();

        PostActions = [];
    }

    private EventHubResourceProvider ResourceProvider { get; }

    private string EventHubName { get; }

    private EventHubData CreateEventHubOptions { get; }

    private IDictionary<string, EventHubConsumerGroupBuilder> ConsumerGroupBuilders { get; }

    private IList<Action<EventHubData>> PostActions { get; }

    /// <summary>
    /// Add an action that will be called after the event hub has been created.
    /// </summary>
    /// <param name="postAction">Action to call with event hub properties when event hub has been created.</param>
    /// <returns>Event hub resouce builder.</returns>
    public EventHubResourceBuilder Do(Action<EventHubData> postAction)
    {
        PostActions.Add(postAction);

        return this;
    }

    /// <summary>
    /// Create event hub according to configured builder.
    /// </summary>
    /// <returns>Instance with information about the created event hub.</returns>
    public async Task<EventHubResource> CreateAsync()
    {
        var eventhubResource = await CreateEventhubResourceAsync().ConfigureAwait(false);

        await CreateConsumerGroupsAsync(eventhubResource).ConfigureAwait(false);

        return eventhubResource;
    }

    /// <inheritdoc/>
    public EventHubConsumerGroupBuilder AddConsumerGroup(string consumerGroupName, string? userMetaData = default)
    {
        var consumerGroupBuilder = new EventHubConsumerGroupBuilder(this, consumerGroupName, userMetaData);
        ConsumerGroupBuilders.Add(consumerGroupName, consumerGroupBuilder);
        return consumerGroupBuilder;
    }

    private async Task<EventHubResource> CreateEventhubResourceAsync()
    {
        ResourceProvider.TestLogger.WriteLine($"Creating event hub '{EventHubName}'");

        var response = await ResourceProvider.EventHubNamespaceResource
            .GetEventHubs()
            .CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                EventHubName,
                CreateEventHubOptions)
            .ConfigureAwait(false);
        var resource = response.Value;

        var eventHubResource = new EventHubResource(ResourceProvider, resource);
        ResourceProvider.EventHubResources.Add(resource.Data.Name, eventHubResource);

        foreach (var postAction in PostActions)
        {
            postAction(resource.Data);
        }

        return eventHubResource;
    }

    private async Task CreateConsumerGroupsAsync(EventHubResource eventHubResource)
    {
        foreach (var consumerGroupBuilderPair in ConsumerGroupBuilders)
        {
            var consumerGroup = consumerGroupBuilderPair.Value;

            var response = await eventHubResource.InnerResource
                .GetEventHubsConsumerGroups()
                .CreateOrUpdateAsync(
                    Azure.WaitUntil.Completed,
                    consumerGroup.ConsumerGroupName,
                    new EventHubsConsumerGroupData
                    {
                        UserMetadata = consumerGroup.UserMetadata,
                    })
                .ConfigureAwait(false);
            var resource = response.Value;

            eventHubResource.AddConsumerGroup(resource);

            foreach (var postAction in consumerGroup.PostActions)
            {
                postAction(resource.Data);
            }
        }
    }
}
