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
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Core.Messaging.UnitTests.Extensions.Builder;

public sealed class ServiceBusHealthCheckBuilderExtensionsTests
{
    private ServiceCollection Services { get; } = new();

    [Fact]
    public void Given_Namespace_Then_HealthCheckAdded()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        // Act
        var act = () => healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => Guid.NewGuid().ToString("N"),
            _ => "topicName",
            _ => "subscriptionName",
            _ => new DefaultAzureCredential(),
            "Some_Health_Check_Name");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    [Obsolete("Obsolete")]
    public void Given_ConnectionString_Then_HealthCheckAdded()
    {
        // Arrange
        var healthChecksBuilder = Services
            .AddLogging()
            .AddHealthChecks();

        // Act
        var act = () => healthChecksBuilder.AddServiceBusTopicSubscriptionDeadLetter(
            _ => Guid.NewGuid().ToString("N"),
            _ => "topicName",
            _ => "subscriptionName",
            "Some_Health_Check_Name");

        // Assert
        act.Should().NotThrow();
    }
}
