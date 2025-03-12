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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        AssertHttpClient(serviceProvider, WorkspaceUrl);
    }

    private static void AssertHttpClient(
        ServiceProvider serviceProvider,
        string workspaceUri)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);

        httpClient.BaseAddress.Should().Be(new Uri(workspaceUri));
    }
}
