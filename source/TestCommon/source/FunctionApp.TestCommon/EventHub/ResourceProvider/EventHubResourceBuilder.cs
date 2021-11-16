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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Management.EventHub;
using Microsoft.Azure.Management.EventHub.Models;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider
{
    /// <summary>
    /// Fluent API for creating an event hub resource.
    /// </summary>
    public class EventHubResourceBuilder
    {
        internal EventHubResourceBuilder(EventHubResourceProvider resourceProvider, string eventHubName, Eventhub createEventHubOptions)
        {
            ResourceProvider = resourceProvider;
            EventHubName = eventHubName;
            CreateEventHubOptions = createEventHubOptions;

            PostActions = new List<Action<Eventhub>>();
        }

        private EventHubResourceProvider ResourceProvider { get; }

        private string EventHubName { get; }

        private Eventhub CreateEventHubOptions { get; }

        private IList<Action<Eventhub>> PostActions { get; }

        /// <summary>
        /// Add an action that will be called after the event hub has been created.
        /// </summary>
        /// <param name="postAction">Action to call with event hub properties when event hub has been created.</param>
        /// <returns>Event hub resouce builder.</returns>
        public EventHubResourceBuilder Do(Action<Eventhub> postAction)
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
            ResourceProvider.TestLogger.WriteLine($"Creating event hub '{EventHubName}'");

            var managementClient = await ResourceProvider.LazyManagementClient
                .ConfigureAwait(false);

            var eventHubNamespace = GetEventHubNamespace(ResourceProvider.ConnectionString);
            var response = await managementClient.EventHubs
                .CreateOrUpdateAsync(
                    ResourceProvider.ResourceManagementSettings.ResourceGroup,
                    eventHubNamespace,
                    EventHubName,
                    CreateEventHubOptions)
                .ConfigureAwait(false);

            var eventHubResource = new EventHubResource(
                ResourceProvider,
                ResourceProvider.ResourceManagementSettings.ResourceGroup,
                eventHubNamespace,
                response);
            ResourceProvider.EventHubResources.Add(response.Name, eventHubResource);

            foreach (var postAction in PostActions)
            {
                postAction(response);
            }

            return eventHubResource;
        }

        private string GetEventHubNamespace(string eventHubConnectionString)
        {
            // The connection string is similar to a service bus connection string.
            // Example connection string: 'Endpoint=sb://xxx.servicebus.windows.net/;'
            var namespaceMatchPattern = @"Endpoint=sb://(.*?).servicebus.windows.net/";
            var match = Regex.Match(eventHubConnectionString, namespaceMatchPattern, RegexOptions.IgnoreCase);
            var eventHubNamespace = match.Groups[1].Value;

            return eventHubNamespace;
        }
    }
}
