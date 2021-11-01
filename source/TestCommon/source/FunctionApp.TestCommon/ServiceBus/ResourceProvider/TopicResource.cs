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
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    public class TopicResource : IAsyncDisposable
    {
        private readonly TopicProperties _properties;
        private readonly Lazy<ServiceBusSender> _lazySenderClient;

        public TopicResource(ServiceBusResourceProvider serviceBusResource, TopicProperties properties)
        {
            ServiceBusResource = serviceBusResource;

            _properties = properties;
            _lazySenderClient = new Lazy<ServiceBusSender>(CreateSenderClient);
        }

        public string Name => _properties.Name;

        public ServiceBusSender SenderClient => _lazySenderClient.Value;

        public bool IsDisposed { get; private set; }

        private ServiceBusResourceProvider ServiceBusResource { get; }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore()
                .ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private ServiceBusSender CreateSenderClient()
        {
            return ServiceBusResource.Client.CreateSender(Name);
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            if (IsDisposed)
            {
                return;
            }

            if (_lazySenderClient.IsValueCreated)
            {
                await _lazySenderClient.Value.DisposeAsync()
                    .ConfigureAwait(false);
            }

            await ServiceBusResource.AdministrationClient.DeleteTopicAsync(Name)
                .ConfigureAwait(false);

            IsDisposed = true;
        }
    }
}
