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
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Extensions.DependencyInjection;

public class HealthChecksExtensionsTests
{
    public HealthChecksExtensionsTests()
    {
        Services = new ServiceCollection();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public async Task TryAddHealthChecks_WhenCalled_RegistrationsArePerformed()
    {
        var healthCheckKey = "MyHealthCheck";

        // Logging is required by HealthCheckService
        Services.AddLogging();

        // Act
        Services.TryAddHealthChecks(
            registrationKey: healthCheckKey,
            (key, builder) =>
            {
                // Any registrations can be performed here
                builder.AddCheck<SimpleHealthCheck>(name: key);
            });

        // Assert
        var serviceProvider = Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        var actualStatus = await healthCheckService.CheckHealthAsync();
        actualStatus.Entries.Should().ContainSingle(entry => entry.Key == healthCheckKey);
    }

    [Fact]
    public void TryAddHealthChecks_WhenCalledWithRegistrationKey_RegistrationsArePerformedWithRegistrationKey()
    {
        var registrationKey = "MyHealthCheck";
        var actualKey = string.Empty;

        // Act
        Services.TryAddHealthChecks(
            registrationKey,
            (key, builder) =>
            {
                actualKey = key;
            });

        // Assert
        actualKey.Should().Be(registrationKey);
    }

    [Fact]
    public void TryAddHealthChecks_WhenCalledMultipleTimesWithSameRegistrationKey_BuilderDelegateIsCalledOnlyOnce()
    {
        var registrationKey = "MyHealthCheck";
        var count = 0;

        Services.TryAddHealthChecks(
            registrationKey,
            (key, builder) =>
            {
                count++;
            });

        // Act
        Services.TryAddHealthChecks(
            registrationKey,
            (key, builder) =>
            {
                count++;
            });

        // Assert
        count.Should().Be(1);
    }

    private class SimpleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
        }
    }
}
