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

using Azure.Identity;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.Messaging.IntegrationTests;

public sealed class ServiceBusHealthCheckBuilderExtensionsTests
{
    private ServiceCollection Services { get; } = new();

    [Fact]
    public async Task Given_Namespace_Then_HealthCheckAdded()
    {
        // Arrange
        await using var serviceBusResourceProvider = GetServiceBusResourceProviderWithNamespace();
        await using var topicResource = await serviceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();

        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        // Act
        var act = () => healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => serviceBusResourceProvider.FullyQualifiedNamespace,
            _ => topicResource.Name,
            _ => topicResource.Subscriptions.First().SubscriptionName,
            _ => new DefaultAzureCredential(),
            "Some_Health_Check_Name");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    [Obsolete("Obsolete")]
    public async Task Given_ConnectionString_Then_HealthCheckAdded()
    {
        // Arrange
        await using var serviceBusResourceProvider = GetServiceBusResourceProviderWithConnectionString();
        await using var topicResource = await serviceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();

        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        // Act
        var act = () => healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => serviceBusResourceProvider.ConnectionString,
            _ => topicResource.Name,
            _ => topicResource.Subscriptions.First().SubscriptionName,
            "Some_Health_Check_Name");

        // Assert
        act.Should().NotThrow();
    }

    private static ServiceBusResourceProvider GetServiceBusResourceProviderWithNamespace()
    {
        return new ServiceBusResourceProvider(
            new TestDiagnosticsLogger(),
            new IntegrationTestConfiguration().ServiceBusFullyQualifiedNamespace);
    }

    [Obsolete("Obsolete")]
    private static ServiceBusResourceProvider GetServiceBusResourceProviderWithConnectionString()
    {
        return new ServiceBusResourceProvider(
            new IntegrationTestConfiguration().ServiceBusConnectionString,
            new TestDiagnosticsLogger());
    }
}
