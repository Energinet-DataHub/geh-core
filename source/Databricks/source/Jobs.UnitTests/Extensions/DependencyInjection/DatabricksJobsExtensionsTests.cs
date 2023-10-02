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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.Jobs.Internal;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.Extensions.DependencyInjection;

public class DatabricksJobsExtensionsTests
{
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
            ["WorkspaceUrl"] = "https://foo.com",
            ["WarehouseId"] = "baz",
            ["WorkspaceToken"] = "bar",
        })
        .Build();

    [Fact]
    public void AddDatabricksJobs_Should_ReturnJobsApiClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabricksJobs(_configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IJobsApiClient>();
        client.Should().BeOfType<JobsApiClient>();
    }

    [Theory]
    [InlineData(false, 0, 23)]
    [InlineData(true, -1, 23)]
    [InlineData(true, 0, 24)]
    public void AddDatabricksJobs_Should_RegisterDatabricksJobsOptions(
        bool shouldThrowException, int startHour, int endHour)
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["WorkspaceUrl"] = "https://foo.com",
                ["WarehouseId"] = "baz",
                ["WorkspaceToken"] = "bar",
                ["DatabricksHealthCheckStartHour"] = startHour.ToString(),
                ["DatabricksHealthCheckEndHour"] = endHour.ToString(),
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddDatabricksJobs(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        var act = () => serviceProvider.GetRequiredService<IJobsApiClient>();
        if (shouldThrowException)
        {
            act.Should().Throw<OptionsValidationException>();
        }
        else
        {
            act.Should().NotThrow();
        }
    }
}
