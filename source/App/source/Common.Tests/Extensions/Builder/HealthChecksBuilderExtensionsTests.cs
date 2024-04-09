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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.Common.Extensions.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Extensions.Builder;

public class HealthChecksBuilderExtensionsTests
{
    public HealthChecksBuilderExtensionsTests()
    {
        Services = new ServiceCollection();

        // Required by HealthCheckService
        Services.AddLogging();
    }

    private ServiceCollection Services { get; }

    [Fact]
    public async Task AddLiveCheck_WhenCalled_LiveHealthCheckIsRegistered()
    {
        // Act
        Services
            .AddHealthChecks()
            .AddLiveCheck();

        // Assert
        var serviceProvider = Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        var actualStatus = await healthCheckService.CheckHealthAsync();
        actualStatus.Entries.Should().ContainSingle(entry => entry.Key == HealthChecksConstants.LiveHealthCheckName);
    }

    [Fact]
    public async Task AddServiceHealthCheck_WhenCalled_ServiceHealthCheckIsRegistered()
    {
        var serviceName = "MyService";

        // Required by ServiceHealthCheck
        Services.AddHttpClient();

        // Act
        Services
            .AddHealthChecks()
            .AddServiceHealthCheck(serviceName, new Uri("https://google.com"));

        // Assert
        var serviceProvider = Services.BuildServiceProvider();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

        var actualStatus = await healthCheckService.CheckHealthAsync();
        actualStatus.Entries.Should().ContainSingle(entry => entry.Key == serviceName);
    }
}
