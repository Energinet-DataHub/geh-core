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

using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.Configuration;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Extensions.DependencyInjection;

public class SubsystemAuthenticationExtensionsTests
{
    public SubsystemAuthenticationExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public void Given_ConfiguredSection_When_AddSubsystemAuthenticationForIsolatedWorker_Then_RegistrationsArePerformed()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{SubsystemAuthenticationOptions.SectionName}:{nameof(SubsystemAuthenticationOptions.ApplicationIdUri)}"] = "notEmpty",
            [$"{SubsystemAuthenticationOptions.SectionName}:{nameof(SubsystemAuthenticationOptions.Issuer)}"] = "notEmpty",
        });

        // Act
        var act = () => Services.AddSubsystemAuthenticationForIsolatedWorker(configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_SectionIsMissing_When_AddSubsystemAuthenticationForIsolatedWorker_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>());

        // Act
        var act = () => Services.AddSubsystemAuthenticationForIsolatedWorker(configuration);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Section 'SubsystemAuthentication' not found in configuration*");
    }

    [Fact]
    public void Given_ApplicationIdUriIsMissing_When_AddSubsystemAuthenticationForIsolatedWorker_Then_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{SubsystemAuthenticationOptions.SectionName}:{nameof(SubsystemAuthenticationOptions.Issuer)}"] = "notEmpty",
        });

        // Act
        var act = () => Services.AddSubsystemAuthenticationForIsolatedWorker(configuration);

        // Assert
        act.Should()
            .Throw<InvalidConfigurationException>()
            .WithMessage("Missing 'ApplicationIdUri'*");
    }

    private IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();
    }
}
