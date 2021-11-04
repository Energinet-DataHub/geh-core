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
using System.Linq;
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
            public async Task When_BuildingQueue_Then_ResourceLifecycleIsHandled()
            {
                // Arrange
                var serviceBusResourceProvider = new ServiceBusResourceProvider(ConnectionString);
                var namePrefix = "queue";
                var environmentVariable = "ENV_NAME";
                ServiceBusSender? senderClient = null;

                try
                {
                    // Act
                    var actualResource = await serviceBusResourceProvider
                        .BuildQueue(namePrefix)
                        .SetEnvironmentVariableToQueueName(environmentVariable)
                        .CreateAsync();

                    // Assert
                    var actualName = actualResource.Name;
                    actualName.Should().StartWith(namePrefix);
                    actualName.Should().Contain(serviceBusResourceProvider.RandomSuffix);

                    var actualEnvironmentValue = Environment.GetEnvironmentVariable(environmentVariable);
                    actualEnvironmentValue.Should().Be(actualName);

                    senderClient = actualResource.SenderClient!;
                    await senderClient.SendMessageAsync(new ServiceBusMessage("hello"));
                }
                finally
                {
                    await serviceBusResourceProvider.DisposeAsync();
                }

                senderClient.IsClosed.Should().BeTrue();
            }

            [Fact]
            public async Task When_BuildingTopicWithSubscription_Then_ResourceLifecycleIsHandled()
            {
                // Arrange
                var serviceBusResourceProvider = new ServiceBusResourceProvider(ConnectionString);
                var namePrefix = "topic";
                var topicEnvironmentVariable = "ENV_TOPIC_NAME";
                var subscription01 = "subscription01";
                var subscriptionEnvironmentVariable01 = "ENV_SUBSCRIPTION_NAME_01";
                var subscription02 = "subscription02";
                var subscriptionEnvironmentVariable02 = "ENV_SUBSCRIPTION_NAME_02";
                ServiceBusSender? senderClient = null;

                try
                {
                    // Act
                    var actualResource = await serviceBusResourceProvider
                        .BuildTopic(namePrefix).SetEnvironmentVariableToTopicName(topicEnvironmentVariable)
                        .AddSubscription(subscription01).SetEnvironmentVariableToSubscriptionName(subscriptionEnvironmentVariable01)
                        .AddSubscription(subscription02).SetEnvironmentVariableToSubscriptionName(subscriptionEnvironmentVariable02)
                        .CreateAsync();

                    // Assert
                    var actualName = actualResource.Name;
                    actualName.Should().StartWith(namePrefix);
                    actualName.Should().Contain(serviceBusResourceProvider.RandomSuffix);

                    actualResource.Subscriptions.Count().Should().Be(2);

                    var actualEnvironmentValue = Environment.GetEnvironmentVariable(topicEnvironmentVariable);
                    actualEnvironmentValue.Should().Be(actualName);

                    actualEnvironmentValue = Environment.GetEnvironmentVariable(subscriptionEnvironmentVariable01);
                    actualEnvironmentValue.Should().Be(subscription01);

                    actualEnvironmentValue = Environment.GetEnvironmentVariable(subscriptionEnvironmentVariable02);
                    actualEnvironmentValue.Should().Be(subscription02);

                    senderClient = actualResource.SenderClient!;
                    await senderClient.SendMessageAsync(new ServiceBusMessage("hello"));
                }
                finally
                {
                    await serviceBusResourceProvider.DisposeAsync();
                }

                senderClient.IsClosed.Should().BeTrue();
            }
        }
    }
}
