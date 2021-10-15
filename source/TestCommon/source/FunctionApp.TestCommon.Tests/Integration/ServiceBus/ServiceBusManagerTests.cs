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
using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.ServiceBus
{
    // PoC on using identities to manage Azure resources (locally run tests as developer, on build agent run tests as SPN).
    public class ServiceBusManagerTests
    {
        public class UsingKeyVaultSecrets
        {
            public UsingKeyVaultSecrets()
            {
                var integrationtestConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("integrationtest.local.settings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                var keyVaultUrl = integrationtestConfiguration.GetValue("AZURE_KEYVAULT_URL");
                var secrets = new ConfigurationBuilder()
                    .AddAuthenticatedAzureKeyVault(keyVaultUrl)
                    .Build();

                ConnectionString = secrets.GetValue("AZURE-SERVICEBUS-CONNECTIONSTRING");
            }

            private string ConnectionString { get; }

            [Fact]
            public async Task When_CreateQueueAsync_Then_QueueWithExpectedNameIsCreated()
            {
                // Arrange
                var manager = new ServiceBusManager(ConnectionString);
                var queueNamePrefix = "queue";

                // Act
                var actualProperties = await manager.CreateQueueAsync(queueNamePrefix);

                // Assert
                actualProperties.Name.Should().StartWith(queueNamePrefix);
                actualProperties.Name.Should().Contain(manager.InstanceName);

                await manager.DeleteQueueAsync(actualProperties.Name);
                await manager.DisposeAsync();
            }
        }
    }
}
