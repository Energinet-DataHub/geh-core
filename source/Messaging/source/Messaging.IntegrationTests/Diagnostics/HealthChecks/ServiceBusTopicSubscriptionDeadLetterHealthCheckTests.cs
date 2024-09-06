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
using Energinet.DataHub.Core.Messaging.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.Messaging.IntegrationTests.Diagnostics.HealthChecks;

public sealed class ServiceBusTopicSubscriptionDeadLetterHealthCheckTests(
    ServiceBusFixture fixture)
    : IClassFixture<ServiceBusFixture>, IAsyncLifetime
{
    private const string HealthCheckName = "Some_Health_Check_Name";

    private ServiceCollection Services { get; } = new();

    private ServiceBusFixture Fixture { get; } = fixture;

    public Task InitializeAsync()
    {
        // No-op
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var receivedMessage = await Fixture.Receiver!.ReceiveMessageAsync();
        if (receivedMessage is not null)
        {
            await Fixture.Receiver!.CompleteMessageAsync(receivedMessage);
        }

        var checkMessageReceiver = await Fixture.Receiver!.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
        if (checkMessageReceiver != null)
        {
            throw new InvalidOperationException("Message was not removed from the topic.");
        }

        var deadLetterMessage = await Fixture.DeadLetterReceiver!.ReceiveMessageAsync();
        if (deadLetterMessage != null)
        {
            await Fixture.DeadLetterReceiver!.CompleteMessageAsync(deadLetterMessage);
        }

        var checkMessageDeadLetter = await Fixture.DeadLetterReceiver!.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
        if (checkMessageDeadLetter != null)
        {
            throw new InvalidOperationException("Message was not removed from the dead letter queue.");
        }
    }

    [Fact]
    public async Task Given_MessageAddedToServiceBusTopic_Then_Healthy()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => Fixture.ServiceBusResourceProvider.FullyQualifiedNamespace,
            _ => Fixture.TopicResource!.Name,
            _ => Fixture.TopicResource!.Subscriptions.First().SubscriptionName,
            _ => Fixture.AzureCredential,
            HealthCheckName);

        // Act
        var sender = Fixture.TopicResource!.SenderClient;
        var message = new ServiceBusMessage("Test message");
        await sender.SendMessageAsync(message);

        // Assert
        var provider = Services.BuildServiceProvider();
        var healthReport = await provider
            .GetRequiredService<HealthCheckService>()
            .CheckHealthAsync();

        healthReport.Status.Should().Be(HealthStatus.Healthy);
        healthReport.Entries.Keys.Should().Contain(HealthCheckName);

        var healthReportEntry = healthReport.Entries[HealthCheckName];
        healthReportEntry.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task Given_MessageAddedToServiceBusDeadLetter_Then_Unhealthy()
    {
        // Arrange
        var healthChecksBuilder = Services.AddLogging()
            .AddHealthChecks();

        healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => Fixture.ServiceBusResourceProvider.FullyQualifiedNamespace,
            _ => Fixture.TopicResource!.Name,
            _ => Fixture.TopicResource!.Subscriptions.First().SubscriptionName,
            _ => Fixture.AzureCredential,
            HealthCheckName);

        // Act
        var sender = Fixture.TopicResource!.SenderClient;
        var message = new ServiceBusMessage("Test message");
        await sender.SendMessageAsync(message);

        var receivedMessage = await Fixture.Receiver!.ReceiveMessageAsync();
        await Fixture.Receiver!.DeadLetterMessageAsync(receivedMessage);

        // Assert
        var provider = Services.BuildServiceProvider();
        var healthReport = await provider
            .GetRequiredService<HealthCheckService>()
            .CheckHealthAsync();

        healthReport.Status.Should().Be(HealthStatus.Unhealthy);
        healthReport.Entries.Keys.Should().Contain(HealthCheckName);

        var healthReportEntry = healthReport.Entries[HealthCheckName];
        healthReportEntry.Status.Should().Be(HealthStatus.Unhealthy);
        healthReportEntry.Description.Should()
            .NotBeNullOrWhiteSpace()
            .And.Contain("has dead-letter messages");
    }
}
