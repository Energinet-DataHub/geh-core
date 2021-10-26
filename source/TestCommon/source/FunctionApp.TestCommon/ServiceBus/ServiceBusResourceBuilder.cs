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

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus
{
    public class ServiceBusResourceBuilder : IAsyncDisposable
    {
        public ServiceBusResourceBuilder(string connectionString)
        {
            ServiceBusManager = new ServiceBusManager(connectionString);
        }

        private ServiceBusManager ServiceBusManager { get; }

        private IDictionary<string, QueueResource> QueueResources { get; }
            = new Dictionary<string, QueueResource>();

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        public async Task<QueueResourceBuilder> BuildQueueResoureceAsync(string queueNamePrefix)
        {
            if (QueueResources.ContainsKey(queueNamePrefix))
            {
                throw new InvalidOperationException($"Invalid queue name prefix. The value '{queueNamePrefix}' has been used before.");
            }

            var queueProperties = await ServiceBusManager.CreateQueueAsync(queueNamePrefix)
                .ConfigureAwait(false);

            var queueResource = new QueueResource(queueProperties.Name);
            QueueResources.Add(queueNamePrefix, queueResource);

            return new QueueResourceBuilder(ServiceBusManager, queueResource);
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            try
            {
                await CleanupQueueResourcesAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                await ServiceBusManager.DisposeAsync()
                    .ConfigureAwait(false);
            }
        }

        private async Task CleanupQueueResourcesAsync()
        {
            foreach (var queueResource in QueueResources)
            {
                if (queueResource.Value.SenderClient != null)
                {
                    await queueResource.Value.SenderClient.DisposeAsync()
                        .ConfigureAwait(false);
                }

                await ServiceBusManager.DeleteQueueAsync(queueResource.Value.Name)
                    .ConfigureAwait(false);
            }
        }
    }
}
