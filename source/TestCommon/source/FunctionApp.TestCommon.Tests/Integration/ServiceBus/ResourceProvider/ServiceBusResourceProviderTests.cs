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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.ServiceBus.ResourceProvider
{
    // PoC on using identities to manage Azure resources (locally run tests as developer, on build agent run tests as SPN).
    public class ServiceBusResourceProviderTests
    {
        public class UsingKeyVaultSecrets
        {
            public UsingKeyVaultSecrets()
            {
                var integrationTestConfiguration = new IntegrationTestConfiguration();
                ConnectionString = integrationTestConfiguration.ServiceBusConnectionString;
            }

            private string ConnectionString { get; }

            [Fact]
            public async Task When_UsingProvider_Then_QueueResourceLifecycleIsHandled()
            {
                // Arrange
                var serviceBusResourceProvider = new ServiceBusResourceProvider(ConnectionString);
                var queueNamePrefix = "queue";

                // Act
                var queueResource = await serviceBusResourceProvider
                    .BuildQueue(queueNamePrefix)
                    .SetEnvironmentVariableToName("test")
                    .CreateAsync();

                var queueName = queueResource.Name;
                var senderClient = queueResource.SenderClient!;
                await senderClient.SendMessageAsync(new ServiceBusMessage("hello"));

                // Assert
                queueName.Should().StartWith(queueNamePrefix);
                queueName.Should().Contain(serviceBusResourceProvider.RandomSuffix);

                await serviceBusResourceProvider.DisposeAsync();

                senderClient.IsClosed.Should().BeTrue();
            }
        }
    }
}
