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
using AutoFixture;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.Management.EventHub.Models;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.EventHub.ResourceProvider
{
    public class EventHubResourceProviderTests
    {
        /// <summary>
        /// Since we are testing <see cref="EventHubResourceProvider.DisposeAsync"/> and the lifecycle
        /// of resources and clients, we do not use the base class here. Instead we have to explicit
        /// dispose so we can verify clients state after.
        /// </summary>
        [Collection(nameof(EventHubResourceProviderCollectionFixture))]
        public class DisposeAsync
        {
            public DisposeAsync(EventHubResourceProviderFixture resourceProviderFixture)
            {
                ResourceProviderFixture = resourceProviderFixture;
            }

            private EventHubResourceProviderFixture ResourceProviderFixture { get; }

            [Fact]
            public async Task When_EventHubResourceIsDisposed_Then_EventHubIsDeletedAndClientIsClosed()
            {
                // Arrange
                var sut = CreateSut();

                var eventHub = await sut
                    .BuildEventHub("eventhub")
                    .CreateAsync();

                using var eventBatch = await CreateFilledEventBatchAsync(eventHub.ProducerClient);
                await eventHub.ProducerClient.SendAsync(eventBatch);

                // Act
                await sut.DisposeAsync();

                // Assert
                Func<Task> act = () => ResourceProviderFixture.ManagementClient.EventHubs.GetWithHttpMessagesAsync(
                    eventHub.ResourceGroup,
                    eventHub.EventHubNamespace,
                    eventHub.Name);
                await act.Should()
                    .ThrowAsync<ErrorResponseException>()
                    .WithMessage("Operation returned an invalid status code 'NotFound'");

                eventHub.ProducerClient.IsClosed.Should().BeTrue();
            }

            private static async Task<EventDataBatch> CreateFilledEventBatchAsync(EventHubProducerClient producerClient, int numberOfEvents = 3)
            {
                var eventBatch = await producerClient.CreateBatchAsync();

                for (var i = 1; i <= numberOfEvents; i++)
                {
                    if (!eventBatch.TryAdd(new EventData($"Event {i}")))
                    {
                        // If it is too large for the batch
                        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                    }
                }

                return eventBatch;
            }

            private EventHubResourceProvider CreateSut()
            {
                return new EventHubResourceProvider(
                    ResourceProviderFixture.ConnectionString,
                    ResourceProviderFixture.ResourceManagementSettings,
                    ResourceProviderFixture.TestLogger);
            }
        }

        /// <summary>
        /// Test whole <see cref="EventHubResourceProvider.BuildEventHub(string)"/> chain
        /// with <see cref="EventHubResourceBuilder"/> and including related extensions.
        ///
        /// A new <see cref="EventHubResourceProvider"/> is created and disposed for each test.
        /// </summary>
        [Collection(nameof(EventHubResourceProviderCollectionFixture))]
        public class BuildEventHub : TestBase<EventHubResourceProvider>, IAsyncLifetime
        {
            private const string NamePrefix = "eventhub";

            public BuildEventHub(EventHubResourceProviderFixture resourceProviderFixture)
            {
                ResourceProviderFixture = resourceProviderFixture;

                // Customize auto fixture
                Fixture.Inject<ITestDiagnosticsLogger>(resourceProviderFixture.TestLogger);
                Fixture.Inject<AzureResourceManagementSettings>(resourceProviderFixture.ResourceManagementSettings);
                Fixture.ForConstructorOn<EventHubResourceProvider>()
                    .SetParameter("connectionString").To(ResourceProviderFixture.ConnectionString);
            }

            private EventHubResourceProviderFixture ResourceProviderFixture { get; }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public async Task DisposeAsync()
            {
                await Sut.DisposeAsync();
            }

            [Fact]
            public async Task When_EventHubNamePrefix_Then_CreatedEventHubNameIsCombinationOfPrefixAndRandomSuffix()
            {
                // Arrange

                // Act
                var actualResource = await Sut
                    .BuildEventHub(NamePrefix)
                    .CreateAsync();

                // Assert
                var actualName = actualResource.Name;
                actualName.Should().StartWith(NamePrefix);
                actualName.Should().EndWith(Sut.RandomSuffix);

                // => Validate the event hub exists
                using var response = await ResourceProviderFixture.ManagementClient.EventHubs.GetWithHttpMessagesAsync(
                    actualResource.ResourceGroup,
                    actualResource.EventHubNamespace,
                    actualResource.Name);
                response.Body.Name.Should().Be(actualName);
            }

            [Fact]
            public async Task When_SetEnvironmentVariable_Then_EnvironmentVariableContainsActualName()
            {
                // Arrange
                var environmentVariable = "ENV_NAME";

                // Act
                var actualResource = await Sut
                    .BuildEventHub(NamePrefix)
                    .SetEnvironmentVariableToEventHubName(environmentVariable)
                    .CreateAsync();

                // Assert
                var actualName = actualResource.Name;

                var actualEnvironmentValue = Environment.GetEnvironmentVariable(environmentVariable);
                actualEnvironmentValue.Should().Be(actualName);
            }

            [Theory]
            [InlineData("some user metadata")]
            [InlineData(null)]
            public async Task When_AddConsumerGroup_Then_CreatedEventHubHasConsumerGroup(string userMetadata)
            {
                // Arrange
                var consumerGroupName = "consumer_group_name";

                // Act
                var actualResource = await Sut
                    .BuildEventHub(NamePrefix)
                    .AddConsumerGroup(consumerGroupName, userMetadata)
                    .CreateAsync();

                // Assert
                using var response = await ResourceProviderFixture.ManagementClient.ConsumerGroups.GetWithHttpMessagesAsync(
                    actualResource.ResourceGroup,
                    actualResource.EventHubNamespace,
                    actualResource.Name,
                    consumerGroupName);

                using var assertionScope = new AssertionScope();
                response.Body.Name.Should().Be(consumerGroupName);
                response.Body.UserMetadata.Should().Be(userMetadata);
            }

            [Fact]
            public async Task When_SetEnvironmentVariableToConsumerGroupName_Then_EnvironmentVariableContainsActualName()
            {
                // Arrange
                const string environmentVariable = "env_consumer_group_name";
                const string consumerGroupName = "consumer_group_name";

                // Act
                var actualResource = await Sut
                    .BuildEventHub(NamePrefix)
                    .AddConsumerGroup(consumerGroupName)
                    .SetEnvironmentVariableToConsumerGroupName(environmentVariable)
                    .CreateAsync();

                // Assert
                var actualEnvironmentValue = Environment.GetEnvironmentVariable(environmentVariable);
                actualEnvironmentValue.Should().Be(consumerGroupName);
            }
        }
    }
}
