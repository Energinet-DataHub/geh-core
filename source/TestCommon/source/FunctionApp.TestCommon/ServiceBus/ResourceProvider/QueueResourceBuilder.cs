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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    /// <summary>
    /// Fluent API for creating a Service Bus queue resource.
    /// </summary>
    public class QueueResourceBuilder
    {
        internal QueueResourceBuilder(ServiceBusResourceProvider serviceBusResource, CreateQueueOptions createQueueOptions)
        {
            ServiceBusResource = serviceBusResource;
            CreateQueueOptions = createQueueOptions;

            PostActions = new List<Action<QueueProperties>>();
        }

        private ServiceBusResourceProvider ServiceBusResource { get; }

        private CreateQueueOptions CreateQueueOptions { get; }

        private IList<Action<QueueProperties>> PostActions { get; }

        /// <summary>
        /// Add an action that will be called after the queue has been created.
        /// </summary>
        /// <param name="postAction">Action to call with queue properties when queue has been created.</param>
        /// <returns>Queue resouce builder.</returns>
        public QueueResourceBuilder Do(Action<QueueProperties> postAction)
        {
            PostActions.Add(postAction);

            return this;
        }

        /// <summary>
        /// Create Service Bus queue according to configured builder.
        /// </summary>
        /// <returns>Instance with information about the created queue.</returns>
        public async Task<QueueResource> CreateAsync()
        {
            ServiceBusResource.TestLogger.WriteLine($"Creating queue '{CreateQueueOptions.Name}'");

            var response = await ServiceBusResource.AdministrationClient.CreateQueueAsync(CreateQueueOptions)
                .ConfigureAwait(false);

            var queueName = response.Value.Name;
            var queueResource = new QueueResource(ServiceBusResource, response.Value);
            ServiceBusResource.QueueResources.Add(queueName, queueResource);

            foreach (var postAction in PostActions)
            {
                postAction(response.Value);
            }

            return queueResource;
        }
    }
}
