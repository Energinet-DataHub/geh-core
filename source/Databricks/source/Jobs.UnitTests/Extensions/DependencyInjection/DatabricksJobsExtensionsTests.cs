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
    [InlineData(0, 23, "")]
    [InlineData(-1, 23, "*DatabricksHealthCheckStartHour must be between 0 and 23*")]
    [InlineData(0, 24, "*DatabricksHealthCheckEndHour must be between 0 and 23*")]
    [InlineData(1, 1, "*end hour must be greater than start hour*")]
    public void AddDatabricksJobs_Should_RegisterDatabricksJobsOptions(
        int startHour, int endHour, string expectedExceptionMessageWildcardPattern)
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
        if (string.IsNullOrEmpty(expectedExceptionMessageWildcardPattern))
        {
            act.Should().NotThrow();
        }
        else
        {
            act.Should()
                .Throw<OptionsValidationException>()
                .WithMessage(expectedWildcardPattern: expectedExceptionMessageWildcardPattern);
        }
    }
}
