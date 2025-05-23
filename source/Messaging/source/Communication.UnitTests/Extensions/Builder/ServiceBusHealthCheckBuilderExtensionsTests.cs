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
using Energinet.DataHub.Core.Messaging.Communication.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Core.Messaging.Communication.UnitTests.Extensions.Builder;

public sealed class ServiceBusHealthCheckBuilderExtensionsTests
{
    private ServiceCollection Services { get; } = new();

    [Fact]
    public void
        Given_FullyQualifiedNamespace_When_AddServiceBusTopicSubscriptionDeadLetter_Then_HealthCheckIsRegistered()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddTokenCredentialProvider()
            .AddHealthChecks();

        // Act
        healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => $"https://{Guid.NewGuid():N}.servicebus.windows.net:8080",
            _ => "topicName",
            _ => "subscriptionName",
            sp => sp.GetRequiredService<TokenCredentialProvider>().Credential,
            "Some_Health_Check_Name");

        // Assert
        var serviceProvider = Services.BuildServiceProvider();

        var healthCheckRegistrations = serviceProvider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations;

        healthCheckRegistrations
            .Should()
            .ContainSingle();

        var healthCheckRegistration = healthCheckRegistrations.Single();

        healthCheckRegistration.Name.Should().Be("Some_Health_Check_Name");
        healthCheckRegistration.Factory(serviceProvider)
            .Should()
            .BeOfType<ServiceBusTopicSubscriptionDeadLetterHealthCheck>();
    }

    [Fact]
    [Obsolete("Obsolete")]
    public void Given_ConnectionString_When_AddServiceBusTopicSubscriptionDeadLetter_Then_HealthCheckIsRegistered()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        // Act
        healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => $"https://{Guid.NewGuid():N}.servicebus.windows.net:8080",
            _ => "topicName",
            _ => "subscriptionName",
            "Some_Health_Check_Name");

        // Assert
        var serviceProvider = Services.BuildServiceProvider();

        var healthCheckRegistrations = serviceProvider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations;

        healthCheckRegistrations
            .Should()
            .ContainSingle();

        var healthCheckRegistration = healthCheckRegistrations.Single();

        healthCheckRegistration.Name.Should().Be("Some_Health_Check_Name");
        healthCheckRegistration.Factory(serviceProvider)
            .Should()
            .BeOfType<ServiceBusTopicSubscriptionDeadLetterHealthCheck>();
    }

    [Fact]
    public void
        Given_FullyQualifiedNamespace_When_AddServiceBusQueueDeadLetter_Then_HealthCheckIsRegistered()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddTokenCredentialProvider()
            .AddHealthChecks();

        // Act
        healthChecksBuilder.AddServiceBusQueueDeadLetter(
            _ => $"https://{Guid.NewGuid():N}.servicebus.windows.net:8080",
            _ => "queueName",
            sp => sp.GetRequiredService<TokenCredentialProvider>().Credential,
            "Some_Health_Check_Name");

        // Assert
        var serviceProvider = Services.BuildServiceProvider();

        var healthCheckRegistrations = serviceProvider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations;

        healthCheckRegistrations
            .Should()
            .ContainSingle();

        var healthCheckRegistration = healthCheckRegistrations.Single();

        healthCheckRegistration.Name.Should().Be("Some_Health_Check_Name");
        healthCheckRegistration.Factory(serviceProvider)
            .Should()
            .BeOfType<ServiceBusQueueDeadLetterHealthCheck>();
    }

    [Fact]
    [Obsolete("Obsolete")]
    public void Given_ConnectionString_When_AddServiceBusQueueDeadLetter_Then_HealthCheckIsRegistered()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        // Act
        healthChecksBuilder.AddServiceBusQueueDeadLetter(
            _ => $"https://{Guid.NewGuid():N}.servicebus.windows.net:8080",
            _ => "queueName",
            "Some_Health_Check_Name");

        // Assert
        var serviceProvider = Services.BuildServiceProvider();

        var healthCheckRegistrations = serviceProvider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations;

        healthCheckRegistrations
            .Should()
            .ContainSingle();

        var healthCheckRegistration = healthCheckRegistrations.Single();

        healthCheckRegistration.Name.Should().Be("Some_Health_Check_Name");
        healthCheckRegistration.Factory(serviceProvider)
            .Should()
            .BeOfType<ServiceBusQueueDeadLetterHealthCheck>();
    }
}
