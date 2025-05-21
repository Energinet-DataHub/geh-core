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

using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.Common.Identity;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Extensions.DependencyInjection;

public class IdentityExtensionsTests
{
    public IdentityExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public void Given_RequiredServicesNotRegisteredAndAddAuthorizationHeaderProvider_When_GetRequiredService_Then_ThrowsException()
    {
        // Arrange
        Services.AddAuthorizationHeaderProvider();

        var serviceProvider = Services.BuildServiceProvider();

        // Act
        var act = () => serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("No service for type 'Energinet.DataHub.Core.App.Common.Identity.TokenCredentialProvider' has been registered*");
    }

    [Fact]
    public void Given_RequiredServices_When_AddAuthorizationHeaderProvider_Then_RegistrationsArePerformed()
    {
        // Arrange
        Services.AddTokenCredentialProvider();

        // Act
        Services.AddAuthorizationHeaderProvider();

        // Assert
        var serviceProvider = Services.BuildServiceProvider();

        var actualAuthorizationHeaderProvider = serviceProvider.GetRequiredService<IAuthorizationHeaderProvider>();
        actualAuthorizationHeaderProvider.Should().NotBeNull();
    }

    [Fact]
    public void Given_RequiredServicesAndAddAuthorizationHeaderProviderWasCalled_When_AddAuthorizationHeaderProvider_Then_RegistrationsArePerformedOnlyOnce()
    {
        // Arrange
        Services.AddTokenCredentialProvider();
        Services.AddAuthorizationHeaderProvider();

        // Act
        Services.AddAuthorizationHeaderProvider();

        // Assert
        var serviceProvider = Services.BuildServiceProvider();

        var actualAuthorizationHeaderProviders = serviceProvider.GetServices<IAuthorizationHeaderProvider>();
        actualAuthorizationHeaderProviders.Count().Should().Be(1);
    }
}
