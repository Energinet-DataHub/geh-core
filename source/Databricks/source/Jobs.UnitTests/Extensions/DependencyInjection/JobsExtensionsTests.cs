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
using Energinet.DataHub.Core.Databricks.Jobs.Internal.Constants;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.Extensions.DependencyInjection;

public class JobsExtensionsTests
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
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetRequiredService<IJobsApiClient>();
        client.Should().BeOfType<JobsApiClient>();
    }

    [Fact]
    public void AddDatabricksJobs_Should_ResolveSqlClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabricksJobs(_configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IJobsApiClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabricksJobs_Should_Resolve_Named_Http_Client()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDatabricksJobs(_configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DatabricksJobsApi);
        httpClient.BaseAddress.Should().Be(new Uri("https://foo.com/api/"));
    }
}
