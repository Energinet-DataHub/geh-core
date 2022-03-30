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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Configuration
{
    public class IntegrationTestConfigurationTests : TestBase<IntegrationTestConfiguration>
    {
        [Fact]
        public void Given_IdentityHasAccess_When_B2CSettings_Then_EachPropertyHasValue()
        {
            // Arrange

            // Act
            var actualValue = Sut.B2CSettings;

            // Assert
            using var assertionScope = new AssertionScope();
            actualValue.Should().NotBeNull();
            actualValue.Tenant.Should().NotBeNullOrEmpty();
            actualValue.ServicePrincipalId.Should().NotBeNullOrEmpty();
            actualValue.ServicePrincipalSecret.Should().NotBeNullOrEmpty();
            actualValue.BackendAppId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Given_IdentityHasAccess_When_ResourceManagementSettings_Then_EachPropertyHasValue()
        {
            // Arrange

            // Act
            var actualValue = Sut.ResourceManagementSettings;

            // Assert
            using var assertionScope = new AssertionScope();
            actualValue.Should().NotBeNull();
            actualValue.TenantId.Should().NotBeNullOrEmpty();
            actualValue.SubscriptionId.Should().NotBeNullOrEmpty();
            actualValue.ResourceGroup.Should().NotBeNullOrEmpty();
            actualValue.ClientId.Should().NotBeNullOrEmpty();
            actualValue.ClientSecret.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Given_IdentityHasAccess_When_ApplicationInsightsInstrumentationKey_Then_HasValue()
        {
            // Arrange

            // Act
            var actualValue = Sut.ApplicationInsightsInstrumentationKey;

            // Assert
            actualValue.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Given_IdentityHasAccess_When_EventHubConnectionString_Then_HasValue()
        {
            // Arrange

            // Act
            var actualValue = Sut.EventHubConnectionString;

            // Assert
            actualValue.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Given_IdentityHasAccess_When_ServiceBusConnectionString_Then_HasValue()
        {
            // Arrange

            // Act
            var actualValue = Sut.ServiceBusConnectionString;

            // Assert
            actualValue.Should().NotBeNullOrEmpty();
        }
    }
}
