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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    public class QueueResourceBuilder
    {
        public QueueResourceBuilder(ServiceBusResourceProvider serviceBusResource, CreateQueueOptions createQueueProperties)
        {
            ServiceBusResource = serviceBusResource;
            CreateQueueProperties = createQueueProperties;
        }

        private ServiceBusResourceProvider ServiceBusResource { get; }

        private CreateQueueOptions CreateQueueProperties { get; }

        public QueueResourceBuilder Do(Action<QueueProperties> postAction)
        {
            // TODO: Implement "Do" actions
            return this;
        }

        public async Task<QueueResource> CreateAsync()
        {
            var response = await ServiceBusResource.AdministrationClient.CreateQueueAsync(CreateQueueProperties)
                .ConfigureAwait(false);

            var queueName = response.Value.Name;
            var queueResource = new QueueResource(ServiceBusResource, response.Value);
            ServiceBusResource.QueueResources.Add(queueName, queueResource);

            return queueResource;
        }
    }
}
