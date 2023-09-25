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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests;

public class DatabricksSqlStatementExecutionExtensionsTests
{
    [Fact]
    [Obsolete]
    public void Deprecate_AddDatabricks_Should_ReturnSqlStatementClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddDatabricks(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetRequiredService<IDatabricksSqlStatementClient>();

        using var assertionScope = new AssertionScope();
        client.Should().BeOfType<DatabricksSqlStatementClient>();
    }

    [Fact]
    public void AddDatabricksSqlStatementExecution_WithMultipleParameters_Should_ReturnConfiguredHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddDatabricksSqlStatementExecution(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        AssertHttpClient(serviceProvider, workspaceUri, workspaceToken);
    }

    [Fact]
    public void AddDatabricksSqlStatementExecution_WithMultipleParameters_Should_ResolveSqlClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddDatabricksSqlStatementExecution(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IDatabricksSqlStatementClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabricksSqlStatementExecution_WithDatabricksOptions_Should_ReturnConfiguredHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var databricksOptions = new DatabricksSqlStatementOptions()
        {
            WorkspaceUrl = "https://foo.com",
            WorkspaceToken = "bar",
            WarehouseId = "baz",
        };

        // Act
        services.AddDatabricksSqlStatementExecution(databricksOptions);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        AssertHttpClient(serviceProvider, databricksOptions.WorkspaceUrl, databricksOptions.WorkspaceToken);
    }

    [Fact]
    public void AddDatabricksSqlStatementExecution_WithDatabricksOptions_Should_ResolveSqlClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var databricksOptions = new DatabricksSqlStatementOptions
        {
            WorkspaceUrl = "https://foo.com",
            WorkspaceToken = "bar",
            WarehouseId = "baz",
        };

        // Act
        services.AddDatabricksSqlStatementExecution(databricksOptions);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IDatabricksSqlStatementClient>();
        client.Should().NotBeNull();
    }

    private static void AssertHttpClient(
        ServiceProvider serviceProvider,
        string workspaceUri,
        string workspaceToken)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);

        httpClient.BaseAddress.Should().Be(new Uri(workspaceUri));
        httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        httpClient.DefaultRequestHeaders.Authorization!.Parameter.Should().Be(workspaceToken);
    }
}
