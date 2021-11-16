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
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.EventHub.ResourceProvider
{
    public class EventHubResourceProviderTests
    {
        private const string NamePrefix = "eventhub";

        public EventHubResourceProviderTests()
        {
        }

        [Fact]
        public async Task When_EventHubNamePrefix_Then_CreatedEventHubNameIsCombinationOfPrefixAndRandomSuffix()
        {
            // Arrange
            var integration = new IntegrationTestConfiguration();
            var sut = new EventHubResourceProvider(
                integration.EventHubConnectionString,
                integration.ResourceManagementSettings,
                new TestDiagnosticsLogger());

            // Act
            var actualResource = await sut
                .BuildEventHub(NamePrefix)
                .CreateAsync();

            // Assert
            var actualName = actualResource.Name;
            actualName.Should().StartWith(NamePrefix);
            actualName.Should().EndWith(sut.RandomSuffix);

            // TODO: Assert eventhub exists
            ////var response = await ResourceProviderFixture.AdministrationClient.QueueExistsAsync(actualName);
            ////response.Value.Should().BeTrue();

            await sut.DisposeAsync();
        }
    }
}
