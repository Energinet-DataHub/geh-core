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
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using FluentAssertions;
using FluentAssertions.Execution;
using Messaging.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Messaging.IntegrationTests;

public sealed class DeadLetterHealthCheckTests(ServiceBusFixture fixture) : IClassFixture<ServiceBusFixture>
{
    private ServiceCollection Services { get; } = new();

    private ServiceBusFixture Fixture { get; } = fixture;

    [Fact]
    public async Task Can_add_health_check_to_service_bus()
    {
        // Arrange
        var topicResource = await Fixture.ServiceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();

        var healthChecksBuilder = Services.AddLogging()
            .AddHealthChecks();

        // Act
        var act = () => healthChecksBuilder.AddServiceBusDeadLetter(
            _ => Fixture.ServiceBusResourceProvider.ConnectionString,
            _ => topicResource.Name,
            _ => topicResource.Subscriptions.First().SubscriptionName,
            "Some_Health_Check_Name");

        act.Should().NotThrow();

        // Assert
        var provider = Services.BuildServiceProvider();

        var healthReport = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        using var assertionScope = new AssertionScope();
        healthReport.Status.Should().Be(HealthStatus.Healthy);
        healthReport.Entries.Keys.Should().Contain("Some_Health_Check_Name");

        var healthReportEntry = healthReport.Entries["Some_Health_Check_Name"];
        healthReportEntry.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Healthy_if_message_added_to_service_bus()
    {
        // Arrange
        var topicResource = await Fixture.ServiceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();

        var healthChecksBuilder = Services.AddLogging()
            .AddHealthChecks();

        var act = () => healthChecksBuilder.AddServiceBusDeadLetter(
            _ => Fixture.ServiceBusResourceProvider.ConnectionString,
            _ => topicResource.Name,
            _ => topicResource.Subscriptions.First().SubscriptionName,
            "Some_Health_Check_Name");

        act.Should().NotThrow();

        // Act
        var sender = topicResource.SenderClient;

        var message = new ServiceBusMessage("Test message");

        await sender.SendMessageAsync(message);

        // Assert
        var provider = Services.BuildServiceProvider();
        var healthReport = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        healthReport.Status.Should().Be(HealthStatus.Healthy);
        healthReport.Entries.Keys.Should().Contain("Some_Health_Check_Name");

        var healthReportEntry = healthReport.Entries["Some_Health_Check_Name"];
        healthReportEntry.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Unhealthy_if_message_added_to_dead_letter()
    {
        // Arrange
        var topicResource = await Fixture.ServiceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();

        var healthChecksBuilder = Services.AddLogging()
            .AddHealthChecks();

        var act = () => healthChecksBuilder.AddServiceBusDeadLetter(
            _ => Fixture.ServiceBusResourceProvider.ConnectionString,
            _ => topicResource.Name,
            _ => topicResource.Subscriptions.First().SubscriptionName,
            "Some_Health_Check_Name");

        act.Should().NotThrow();

        // Act
        var sender = topicResource.SenderClient;
        await using var client = new ServiceBusClient(Fixture.ServiceBusResourceProvider.ConnectionString);
        await using var receiver = client.CreateReceiver(topicResource.Name, topicResource.Subscriptions.First().SubscriptionName);

        var message = new ServiceBusMessage("Test message");

        await sender.SendMessageAsync(message);

        var receivedMessage = await receiver.ReceiveMessageAsync();
        await receiver.DeadLetterMessageAsync(receivedMessage);

        // Assert
        var provider = Services.BuildServiceProvider();
        var healthReport = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        healthReport.Status.Should().Be(HealthStatus.Unhealthy);
        healthReport.Entries.Keys.Should().Contain("Some_Health_Check_Name");

        var healthReportEntry = healthReport.Entries["Some_Health_Check_Name"];
        healthReportEntry.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
