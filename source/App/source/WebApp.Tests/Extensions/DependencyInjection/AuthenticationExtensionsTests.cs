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

using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.WebApp.Extensions.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace Energinet.DataHub.Core.App.WebApp.Tests.Extensions.DependencyInjection;

public class AuthenticationExtensionsTests
{
    public AuthenticationExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public void AddJwtBearerAuthenticationForWebApp_WhenCalledWithConfiguredSection_RegistrationsArePerformed()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.MitIdExternalMetadataAddress)}"] = "notEmpty",
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.ExternalMetadataAddress)}"] = "notEmpty",
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.BackendBffAppId)}"] = "notEmpty",
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.InternalMetadataAddress)}"] = "notEmpty",
        });

        // Act
        Services.AddJwtBearerAuthenticationForWebApp(configuration);
    }

    [Fact]
    public void AddJwtBearerAuthenticationForWebApp_WhenCalledAndNoConfiguredSection_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>());

        // Act
        var act = () => Services.AddJwtBearerAuthenticationForWebApp(configuration);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Section 'Authentication' not found in configuration*");
    }

    [Fact]
    public void AddJwtBearerAuthenticationForWebApp_WhenCalledAndConfiguredPropertyIsMissing_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.ExternalMetadataAddress)}"] = "notEmpty",
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.BackendBffAppId)}"] = "notEmpty",
            [$"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.InternalMetadataAddress)}"] = "notEmpty",
        });

        // Act
        var act = () => Services.AddJwtBearerAuthenticationForWebApp(configuration);

        // Assert
        act.Should()
            .Throw<InvalidConfigurationException>()
            .WithMessage("Missing 'MitIdExternalMetadataAddress'*");
    }

    private IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();
    }
}
