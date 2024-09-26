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

using Azure;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.EventHub.ResourceProvider;

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
            var act = () => ResourceProviderFixture.EventHubNamespaceResource.GetEventHub(eventHub.Name);
            act.Should()
                .Throw<RequestFailedException>()
                .WithMessage("EventHub entity does not exist*");

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
                ResourceProviderFixture.TestLogger,
                ResourceProviderFixture.NamespaceName,
                ResourceProviderFixture.ResourceManagementSettings);
        }
    }

    /// <summary>
    /// Test whole <see cref="EventHubResourceProvider.BuildEventHub(string)"/> chain
    /// with <see cref="EventHubResourceBuilder"/> and including related extensions.
    ///
    /// A new <see cref="EventHubResourceProvider"/> is created and disposed for each test.
    /// </summary>
    [Collection(nameof(EventHubResourceProviderCollectionFixture))]
    public class BuildEventHub : EventHubResourceProviderTestsBase
    {
        private const string NamePrefix = "eventhub";

        public BuildEventHub(EventHubResourceProviderFixture resourceProviderFixture)
            : base(resourceProviderFixture)
        {
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
            var actualEventHubResource = ResourceProviderFixture.EventHubNamespaceResource.GetEventHub(actualResource.Name);
            actualEventHubResource.Value.Data.Name.Should().Be(actualName);
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
        public async Task When_AddConsumerGroup_Then_CreatedEventHubHasConsumerGroup(string? userMetadata)
        {
            // Arrange
            var consumerGroupName = "consumer_group_name";

            // Act
            var actualResource = await Sut
                .BuildEventHub(NamePrefix)
                .AddConsumerGroup(consumerGroupName, userMetadata)
                .CreateAsync();

            // Assert
            var actualEventHubResource = ResourceProviderFixture.EventHubNamespaceResource.GetEventHub(actualResource.Name);
            var actualConsumerGroupResource = actualEventHubResource.Value.GetEventHubsConsumerGroup(consumerGroupName);

            using var assertionScope = new AssertionScope();
            actualConsumerGroupResource.Value.Data.Name.Should().Be(consumerGroupName);
            actualConsumerGroupResource.Value.Data.UserMetadata.Should().Be(userMetadata);
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

    /// <summary>
    /// A new <see cref="EventHubResourceProvider"/> is created and disposed for each test.
    /// </summary>
    public abstract class EventHubResourceProviderTestsBase : IAsyncLifetime
    {
        protected EventHubResourceProviderTestsBase(EventHubResourceProviderFixture resourceProviderFixture)
        {
            ResourceProviderFixture = resourceProviderFixture;
            Sut = new EventHubResourceProvider(
                ResourceProviderFixture.TestLogger,
                ResourceProviderFixture.NamespaceName,
                ResourceProviderFixture.ResourceManagementSettings);
        }

        protected EventHubResourceProviderFixture ResourceProviderFixture { get; }

        protected EventHubResourceProvider Sut { get; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await Sut.DisposeAsync();
        }
    }
}
