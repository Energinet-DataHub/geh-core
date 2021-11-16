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
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
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
            private const string DefaultBody = "valid body";

            public DisposeAsync(EventHubResourceProviderFixture resourceProviderFixture)
            {
                ResourceProviderFixture = resourceProviderFixture;

                ////// Customize auto fixture
                ////Fixture = new Fixture();
                ////Fixture.Customize<ServiceBusMessage>(composer => composer
                ////    .OmitAutoProperties()
                ////    .With(p => p.MessageId)
                ////    .With(p => p.Subject)
                ////    .With(p => p.Body, new BinaryData(DefaultBody)));
            }

            private EventHubResourceProviderFixture ResourceProviderFixture { get; }

            ////private IFixture Fixture { get; }

            [Fact]
            public async Task When_EventHubResourceIsDisposed_Then_EventHubIsDeletedAndClientIsClosed()
            {
                // Arrange
                var sut = CreateSut();

                var actualResource = await sut
                    .BuildEventHub("eventhub")
                    .CreateAsync();

                ////var senderClient = actualResource.SenderClient;
                ////var message = Fixture.Create<ServiceBusMessage>();
                ////await senderClient.SendMessageAsync(message);

                // Act
                await sut.DisposeAsync();

                // Assert
                Func<Task> act = () => ResourceProviderFixture.ManagementClient.EventHubs.GetWithHttpMessagesAsync(
                    actualResource.ResourceGroup,
                    actualResource.EventHubNamespace,
                    actualResource.Name);
                await act.Should()
                    .ThrowAsync<ErrorResponseException>()
                    .WithMessage("Operation returned an invalid status code 'NotFound'");

                ////senderClient.IsClosed.Should().BeTrue();
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
                    .SetEnvironmentVariableToQueueName(environmentVariable)
                    .CreateAsync();

                // Assert
                var actualName = actualResource.Name;

                var actualEnvironmentValue = Environment.GetEnvironmentVariable(environmentVariable);
                actualEnvironmentValue.Should().Be(actualName);
            }
        }
    }
}
