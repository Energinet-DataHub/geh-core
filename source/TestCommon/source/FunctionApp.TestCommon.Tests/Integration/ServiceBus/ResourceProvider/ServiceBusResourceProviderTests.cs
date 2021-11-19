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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.ServiceBus.ResourceProvider
{
    public class ServiceBusResourceProviderTests
    {
        /// <summary>
        /// Since we are testing <see cref="ServiceBusResourceProvider.DisposeAsync"/> and the lifecycle
        /// of resources and clients, we do not use the base class here. Instead we have to explicit
        /// dispose so we can verify clients state after.
        /// </summary>
        [Collection(nameof(ServiceBusResourceProviderCollectionFixture))]
        public class DisposeAsync
        {
            private const string DefaultBody = "valid body";

            public DisposeAsync(ServiceBusResourceProviderFixture resourceProviderFixture)
            {
                ResourceProviderFixture = resourceProviderFixture;

                // Customize auto fixture
                Fixture = new Fixture();
                Fixture.Customize<ServiceBusMessage>(composer => composer
                    .OmitAutoProperties()
                    .With(p => p.MessageId)
                    .With(p => p.Subject)
                    .With(p => p.Body, new BinaryData(DefaultBody)));
            }

            private ServiceBusResourceProviderFixture ResourceProviderFixture { get; }

            private IFixture Fixture { get; }

            [Fact]
            public async Task When_QueueResourceIsDisposed_Then_QueueIsDeletedAndClientIsClosed()
            {
                // Arrange
                var sut = CreateSut();

                var actualResource = await sut
                    .BuildQueue("queue")
                    .CreateAsync();

                var actualName = actualResource.Name;

                var senderClient = actualResource.SenderClient;
                var message = Fixture.Create<ServiceBusMessage>();
                await senderClient.SendMessageAsync(message);

                // Act
                await sut.DisposeAsync();

                // Assert
                var response = await ResourceProviderFixture.AdministrationClient.QueueExistsAsync(actualName);
                response.Value.Should().BeFalse();

                senderClient.IsClosed.Should().BeTrue();
            }

            [Fact]
            public async Task When_TopicResourceIsDisposed_Then_TopicAndSubscriptionsAreDeletedAndClientIsClosed()
            {
                // Arrange
                var sut = CreateSut();

                var actualResource = await sut
                    .BuildTopic("topic")
                    .AddSubscription("subscription01")
                    .AddSubscription("subscription02")
                    .CreateAsync();

                var topicName = actualResource.Name;

                var senderClient = actualResource.SenderClient;
                var message = Fixture.Create<ServiceBusMessage>();
                await senderClient.SendMessageAsync(message);

                // Act
                await sut.DisposeAsync();

                // Assert
                var response = await ResourceProviderFixture.AdministrationClient.TopicExistsAsync(topicName);
                response.Value.Should().BeFalse();

                foreach (var subscription in actualResource.Subscriptions)
                {
                    response = await ResourceProviderFixture.AdministrationClient.SubscriptionExistsAsync(topicName, subscription.SubscriptionName);
                    response.Value.Should().BeFalse();
                }

                senderClient.IsClosed.Should().BeTrue();
            }

            private ServiceBusResourceProvider CreateSut()
            {
                return new ServiceBusResourceProvider(
                    ResourceProviderFixture.ConnectionString,
                    ResourceProviderFixture.TestLogger);
            }
        }

        /// <summary>
        /// Test whole <see cref="ServiceBusResourceProvider.BuildQueue(string, int, TimeSpan?, bool)"/> chain
        /// with <see cref="QueueResourceBuilder"/> and including related extensions.
        /// </summary>
        [Collection(nameof(ServiceBusResourceProviderCollectionFixture))]
        public class BuildQueue : ServiceBusResourceProviderTestsBase
        {
            private const string NamePrefix = "queue";

            public BuildQueue(ServiceBusResourceProviderFixture resourceProviderFixture)
                : base(resourceProviderFixture)
            {
            }

            [Fact]
            public async Task When_QueueNamePrefix_Then_CreatedQueueNameIsCombinationOfPrefixAndRandomSuffix()
            {
                // Arrange

                // Act
                var actualResource = await Sut
                    .BuildQueue(NamePrefix)
                    .CreateAsync();

                // Assert
                var actualName = actualResource.Name;
                actualName.Should().StartWith(NamePrefix);
                actualName.Should().EndWith(Sut.RandomSuffix);

                var response = await ResourceProviderFixture.AdministrationClient.QueueExistsAsync(actualName);
                response.Value.Should().BeTrue();
            }

            [Fact]
            public async Task When_SetEnvironmentVariable_Then_EnvironmentVariableContainsActualName()
            {
                // Arrange
                var environmentVariable = "ENV_NAME";

                // Act
                var actualResource = await Sut
                    .BuildQueue(NamePrefix)
                    .SetEnvironmentVariableToQueueName(environmentVariable)
                    .CreateAsync();

                // Assert
                var actualName = actualResource.Name;

                var actualEnvironmentValue = Environment.GetEnvironmentVariable(environmentVariable);
                actualEnvironmentValue.Should().Be(actualName);
            }
        }

        /// <summary>
        /// Test whole <see cref="ServiceBusResourceProvider.BuildTopic(string)"/> chain
        /// with <see cref="TopicResourceBuilder"/>, <see cref="TopicSubscriptionBuilder"/>
        /// and including related extensions.
        /// </summary>
        [Collection(nameof(ServiceBusResourceProviderCollectionFixture))]
        public class BuildTopic : ServiceBusResourceProviderTestsBase
        {
            private const string NamePrefix = "topic";
            private const string SubscriptionName01 = "subscription01";
            private const string SubscriptionName02 = "subscription02";

            public BuildTopic(ServiceBusResourceProviderFixture resourceProviderFixture)
                : base(resourceProviderFixture)
            {
            }

            [Fact]
            public async Task When_TopicNamePrefix_Then_CreatedTopicNameIsCombinationOfPrefixAndRandomSuffix()
            {
                // Arrange

                // Act
                var actualResource = await Sut
                    .BuildTopic(NamePrefix)
                    .CreateAsync();

                // Assert
                var actualName = actualResource.Name;
                actualName.Should().StartWith(NamePrefix);
                actualName.Should().EndWith(Sut.RandomSuffix);

                var response = await ResourceProviderFixture.AdministrationClient.TopicExistsAsync(actualName);
                response.Value.Should().BeTrue();
            }

            [Fact]
            public async Task When_AddSubscriptionName_Then_CreatedSubscriptionHasSubscriptionName()
            {
                // Arrange

                // Act
                var actualResource = await Sut
                    .BuildTopic(NamePrefix)
                    .AddSubscription(SubscriptionName01)
                    .CreateAsync();

                // Assert
                var topicName = actualResource.Name;

                var response = await ResourceProviderFixture.AdministrationClient.SubscriptionExistsAsync(topicName, SubscriptionName01);
                response.Value.Should().BeTrue();
            }

            [Fact]
            public async Task When_AddMultipleSubscriptionNames_Then_CreatedSubscriptionsHasSubscriptionNames()
            {
                // Arrange

                // Act
                var actualResource = await Sut
                    .BuildTopic(NamePrefix)
                    .AddSubscription(SubscriptionName01)
                    .AddSubscription(SubscriptionName02)
                    .CreateAsync();

                // Assert
                var topicName = actualResource.Name;

                actualResource.Subscriptions.Count.Should().Be(2);

                foreach (var subscription in actualResource.Subscriptions)
                {
                    var response = await ResourceProviderFixture.AdministrationClient.SubscriptionExistsAsync(topicName, subscription.SubscriptionName);
                    response.Value.Should().BeTrue();
                }
            }

            [Fact]
            public async Task When_SetEnvironmentVariables_Then_EnvironmentVariablesContainActualNames()
            {
                // Arrange
                var topicEnvironmentVariable = "ENV_TOPIC_NAME";
                var subscriptionEnvironmentVariable01 = "ENV_SUBSCRIPTION_NAME_01";
                var subscriptionEnvironmentVariable02 = "ENV_SUBSCRIPTION_NAME_02";

                // Act
                var actualResource = await Sut
                    .BuildTopic(NamePrefix).SetEnvironmentVariableToTopicName(topicEnvironmentVariable)
                    .AddSubscription(SubscriptionName01).SetEnvironmentVariableToSubscriptionName(subscriptionEnvironmentVariable01)
                    .AddSubscription(SubscriptionName02).SetEnvironmentVariableToSubscriptionName(subscriptionEnvironmentVariable02)
                    .CreateAsync();

                // Assert
                var topicName = actualResource.Name;

                var actualEnvironmentValue = Environment.GetEnvironmentVariable(topicEnvironmentVariable);
                actualEnvironmentValue.Should().Be(topicName);

                actualEnvironmentValue = Environment.GetEnvironmentVariable(subscriptionEnvironmentVariable01);
                actualEnvironmentValue.Should().Be(SubscriptionName01);

                actualEnvironmentValue = Environment.GetEnvironmentVariable(subscriptionEnvironmentVariable02);
                actualEnvironmentValue.Should().Be(SubscriptionName02);
            }
        }

        /// <summary>
        /// A new <see cref="ServiceBusResourceProvider"/> is created and disposed for each test.
        /// </summary>
        public abstract class ServiceBusResourceProviderTestsBase : TestBase<ServiceBusResourceProvider>, IAsyncLifetime
        {
            protected ServiceBusResourceProviderTestsBase(ServiceBusResourceProviderFixture resourceProviderFixture)
            {
                ResourceProviderFixture = resourceProviderFixture;

                // Customize auto fixture
                Fixture.Inject<ITestDiagnosticsLogger>(resourceProviderFixture.TestLogger);
                Fixture.ForConstructorOn<ServiceBusResourceProvider>()
                    .SetParameter("connectionString").To(ResourceProviderFixture.ConnectionString);
            }

            protected ServiceBusResourceProviderFixture ResourceProviderFixture { get; }

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
}
