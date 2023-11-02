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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Extensions.DependencyInjection;

public class DatabricksSqlStatementExecutionExtensionsTests
{
    private const string WorkspaceUrl = "https://foo.com";
    private const string WarehouseId = "baz";
    private const string WorkspaceToken = "bar";

    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>()
        {
            ["WorkspaceUrl"] = WorkspaceUrl,
            ["WarehouseId"] = WarehouseId,
            ["WorkspaceToken"] = WorkspaceToken,
        })
        .Build();

    [Fact]
    public void AddDatabricksSqlStatementExecution_Should_ReturnConfiguredHttpClient()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddDatabricksSqlStatementExecution(_configuration);

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        AssertHttpClient(serviceProvider, WorkspaceUrl, WorkspaceToken);
    }

    [Theory]
    [InlineData(0, 23, "")]
    [InlineData(-1, 23, "*DatabricksHealthCheckStartHour must be between 0 and 23*")]
    [InlineData(0, 24, "*DatabricksHealthCheckEndHour must be between 0 and 23*")]
    [InlineData(1, 1, "*end hour must be greater than start hour*")]
    public void AddDatabricksSqlStatementExecution_Should_RegisterDatabricksSqlStatementOptions(
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
        services.AddDatabricksSqlStatementExecution(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var act = () => serviceProvider.GetRequiredService<DatabricksSqlWarehouseQueryExecutor>();
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
