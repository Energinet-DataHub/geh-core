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
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.ServiceBus
{
    public class ServiceBusManagerTests
    {
        public class UsingManagedIdentity
        {
            public UsingManagedIdentity()
            {
                var integrationtestConfiguration = new ConfigurationBuilder()
                    .AddJsonFile("integrationtest.local.settings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();

                FullyQualifiedNamespace = integrationtestConfiguration.GetValue("AZURE_SERVICEBUS_NAMESPACE");
            }

            private string FullyQualifiedNamespace { get; }

            [Fact]
            public async Task When_CreateQueueAsync_Then_QueueWithExpectedNameIsCreated()
            {
                // Arrange
                var credential = new DefaultAzureCredential();
                var administrationClient = new ServiceBusAdministrationClient(FullyQualifiedNamespace, credential);
                var queueName = $"queue-{Guid.NewGuid()}";
                var createQueueProperties = new CreateQueueOptions(queueName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5),
                    MaxDeliveryCount = 1,
                    RequiresSession = false,
                };

                // Act
                var actualProperties = await administrationClient.CreateQueueAsync(createQueueProperties);

                // Assert
                actualProperties.Value.Name.Should().Be(queueName);
            }
        }
    }
}
