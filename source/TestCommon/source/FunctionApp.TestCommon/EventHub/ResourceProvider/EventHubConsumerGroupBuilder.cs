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

using Microsoft.Azure.Management.EventHub.Models;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;

public class EventHubConsumerGroupBuilder : IEventHubResourceBuilder
{
    internal EventHubConsumerGroupBuilder(EventHubResourceBuilder eventHubResourceBuilder, string consumerGroupName, string? userMetadata = default)
    {
        EventHubResourceBuilder = eventHubResourceBuilder;
        ConsumerGroupName = consumerGroupName;
        UserMetadata = userMetadata;

        PostActions = new List<Action<ConsumerGroup>>();
    }

    internal string ConsumerGroupName { get; }

    internal string? UserMetadata { get; }

    internal IList<Action<ConsumerGroup>> PostActions { get; }

    private EventHubResourceBuilder EventHubResourceBuilder { get; }

    /// <summary>
    /// Add an action that will be called after the consumer group has been created.
    /// </summary>
    /// <param name="postAction">Action to call with consumer group name and metadata when it has been created.</param>
    /// <returns>Consumer group builder.</returns>
    public EventHubConsumerGroupBuilder Do(Action<ConsumerGroup> postAction)
    {
        PostActions.Add(postAction);

        return this;
    }

    /// <inheritdoc/>
    public EventHubConsumerGroupBuilder AddConsumerGroup(string consumerGroupName, string? userMetaData = default)
    {
        return EventHubResourceBuilder.AddConsumerGroup(consumerGroupName, userMetaData);
    }

    /// <inheritdoc/>
    public Task<EventHubResource> CreateAsync()
    {
        return EventHubResourceBuilder.CreateAsync();
    }
}
