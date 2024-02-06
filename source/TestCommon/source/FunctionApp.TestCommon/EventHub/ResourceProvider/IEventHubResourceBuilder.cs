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

using System.Threading.Tasks;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;

/// <summary>
/// Part of fluent API for creating an Event Hub with consumer groups.
/// </summary>
public interface IEventHubResourceBuilder
{
    /// <summary>
    /// Add a Consumer Group to the Event Hub being created.
    /// </summary>
    /// <param name="consumerGroupName">Name of consumer group to add.</param>
    /// <param name="userMetaData">Metadata for consumer group to add (optional).</param>
    /// <returns>EventHub consumer group builder.</returns>
    EventHubConsumerGroupBuilder AddConsumerGroup(string consumerGroupName, string? userMetaData = default);

    /// <summary>
    /// Create event hub according to configured builder.
    /// </summary>
    /// <returns>Instance with information about the created event hub.</returns>
    Task<EventHubResource> CreateAsync();
}
