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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Core.Messaging.Communication.UnitTests.Extensions.DependencyInjection;

public class ServiceBusExtensionsTests
{
    private ServiceCollection Services { get; } = new();

    [Fact]
    public void AddServiceBusClientForApplication_WhenCalledWithConfiguredSection_ServicesCanBeCreated()
    {
        // Arrange
        var fullyQualifiedNamespace = "namespace.servicebus.windows.net";
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}"] = fullyQualifiedNamespace,
        });

        // Act
        Services.AddServiceBusClientForApplication(configuration);

        var serviceProvider = Services.BuildServiceProvider();
        var actualClient = serviceProvider.GetRequiredService<ServiceBusClient>();
        var actualOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>();

        // Assert
        using var assertionScope = new AssertionScope();
        actualClient.FullyQualifiedNamespace.Should().Be(fullyQualifiedNamespace);
        actualOptions.Value.FullyQualifiedNamespace.Should().Be(fullyQualifiedNamespace);
    }

    [Fact]
    public void AddServiceBusClientForApplication_WhenCalledAndNoConfiguredSection_ExceptionIsThrown()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations([]);

        // Act
        var act = () => Services.AddServiceBusClientForApplication(configuration);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Section 'ServiceBus' not found in configuration*");
    }

    /// <summary>
    /// This test documents what might be a surprise for some.
    /// During registration we will NOT get an exception if the required property
    /// <see cref="ServiceBusNamespaceOptions.FullyQualifiedNamespace"/> is empty.
    /// The reason for this is that validation of this value wont happen until runtime
    /// when the registered client is about to be created.
    /// </summary>
    [Fact]
    public void AddServiceBusClientForApplication_WhenCalledAndRequiredPropertyValueIsMissing_RegistrationsArePerformedButCreationShouldThrowException()
    {
        // Arrange
        var configuration = CreateInMemoryConfigurations(new Dictionary<string, string?>()
        {
            [$"{ServiceBusNamespaceOptions.SectionName}:{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}"] = string.Empty,
        });

        // Act
        Services.AddServiceBusClientForApplication(configuration);

        var serviceProvider = Services.BuildServiceProvider();
        var clientAct = serviceProvider.GetRequiredService<ServiceBusClient>;
        var actualOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>();

        // Assert
        using var assertionScope = new AssertionScope();

        clientAct.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value '' is not a well-formed Service Bus fully qualified namespace*");

        var validationAct = () => actualOptions.Value;
        validationAct.Should()
            .Throw<OptionsValidationException>()
            .WithMessage("*The FullyQualifiedNamespace field is required*");
    }

    protected IConfiguration CreateInMemoryConfigurations(Dictionary<string, string?> configurations)
    {
        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurations)
            .Build();

        Services.AddScoped<IConfiguration>(_ => configurationRoot);

        return configurationRoot;
    }
}
