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
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Management.EventHub;
using Microsoft.Azure.Management.EventHub.Models;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider
{
    public class EventHubResource : IAsyncDisposable
    {
        private readonly Eventhub _properties;
        private readonly Lazy<EventHubProducerClient> _lazyProducerClient;

        internal EventHubResource(EventHubResourceProvider resourceProvider, Eventhub properties)
        {
            ResourceProvider = resourceProvider;

            _properties = properties;
            _lazyProducerClient = new Lazy<EventHubProducerClient>(CreateProducerClient);
        }

        public string ResourceGroup => ResourceProvider.ResourceManagementSettings.ResourceGroup;

        public string EventHubNamespace => ResourceProvider.EventHubNamespace;

        public string Name => _properties.Name;

        public EventHubProducerClient ProducerClient => _lazyProducerClient.Value;

        public bool IsDisposed { get; private set; }

        private EventHubResourceProvider ResourceProvider { get; }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore()
                .ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private EventHubProducerClient CreateProducerClient()
        {
            return new EventHubProducerClient(ResourceProvider.ConnectionString, Name);
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            if (IsDisposed)
            {
                return;
            }

            if (_lazyProducerClient.IsValueCreated)
            {
                await _lazyProducerClient.Value.DisposeAsync()
                    .ConfigureAwait(false);
            }

            var managementClient = await ResourceProvider.LazyManagementClient
                .ConfigureAwait(false);

            await managementClient.EventHubs.DeleteAsync(ResourceGroup, EventHubNamespace, Name)
                .ConfigureAwait(false);

            IsDisposed = true;
        }
    }
}
