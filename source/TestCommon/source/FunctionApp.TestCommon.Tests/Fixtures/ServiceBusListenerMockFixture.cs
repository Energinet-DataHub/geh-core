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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Squadron;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures
{
    public class ServiceBusListenerMockFixture : IAsyncLifetime
    {
        public ServiceBusListenerMockFixture(IMessageSink messageSink)
        {
            ServiceBusResource = new AzureCloudServiceBusResource<ServiceBusListenerMockServiceBusOptions>(messageSink);
            SubscriptionName = ServiceBusListenerMockServiceBusOptions.SubscriptionName;
        }

        public string ConnectionString { get; private set; }
            = string.Empty;

        public string QueueName { get; private set; }
            = string.Empty;

        [NotNull]
        public ServiceBusSender? QueueSenderClient { get; private set; }

        public string TopicName { get; private set; }
            = string.Empty;

        public string SubscriptionName { get; private set; }
            = string.Empty;

        [NotNull]
        public ServiceBusSender? TopicSenderClient { get; private set; }

        private AzureCloudServiceBusResource<ServiceBusListenerMockServiceBusOptions> ServiceBusResource { get; }

        [NotNull]
        private ServiceBusClient? Client { get; set; }

        public async Task InitializeAsync()
        {
            await ServiceBusResource.InitializeAsync();

            ConnectionString = ServiceBusResource.ConnectionString;

            Client = new ServiceBusClient(ConnectionString);

            QueueName = ServiceBusListenerMockServiceBusOptions.QueueName;
            QueueSenderClient = Client.CreateSender(QueueName);

            TopicName = ServiceBusListenerMockServiceBusOptions.TopicName;
            TopicSenderClient = Client.CreateSender(TopicName);
        }

        public async Task DisposeAsync()
        {
            await QueueSenderClient!.CloseAsync();
            await TopicSenderClient!.CloseAsync();
            await Client.DisposeAsync();

            await ServiceBusResource.DisposeAsync();
        }
    }
}
