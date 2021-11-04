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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures
{
    public class ServiceBusListenerMockFixture : IAsyncLifetime
    {
        public ServiceBusListenerMockFixture()
        {
            var integrationTestConfiguration = new IntegrationTestConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(integrationTestConfiguration.ServiceBusConnectionString);
        }

        public string ConnectionString => ServiceBusResourceProvider.ConnectionString;

        public QueueResource? Queue { get; private set; }

        public TopicResource? Topic { get; private set; }

        public static string SubscriptionName => "defaultSubscription";

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        public async Task InitializeAsync()
        {
            Queue = await ServiceBusResourceProvider
                .BuildQueue("queue")
                .CreateAsync();

            Topic = await ServiceBusResourceProvider
                .BuildTopic("topic")
                .AddSubscription(SubscriptionName)
                .CreateAsync();
        }

        public async Task DisposeAsync()
        {
            await ServiceBusResourceProvider.DisposeAsync();
        }
    }
}
