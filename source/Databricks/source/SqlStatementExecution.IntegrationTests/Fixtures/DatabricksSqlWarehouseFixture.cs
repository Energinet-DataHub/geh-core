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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Fixtures;

public sealed class DatabricksSqlWarehouseFixture
{
    private static readonly Lazy<IntegrationTestConfiguration> _lazyConfiguration = new(() => new IntegrationTestConfiguration());

    public DatabricksSqlWarehouseQueryExecutor CreateSqlStatementClient()
    {
        var services = CreateServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<DatabricksSqlWarehouseQueryExecutor>();
    }

    public HttpClient CreateHttpClient()
    {
        var services = CreateServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        return factory.CreateClient("Databricks");
    }

    private static ServiceCollection CreateServiceCollection()
    {
        var integrationTestConfiguration = _lazyConfiguration.Value;
        var config = new Dictionary<string, string?>
        {
            { $"{DatabricksSqlStatementOptions.DatabricksOptions}:WorkspaceUrl", integrationTestConfiguration.DatabricksSettings.WorkspaceUrl },
            { $"{DatabricksSqlStatementOptions.DatabricksOptions}:WorkspaceToken", integrationTestConfiguration.DatabricksSettings.WorkspaceAccessToken },
            { $"{DatabricksSqlStatementOptions.DatabricksOptions}:WarehouseId", integrationTestConfiguration.DatabricksSettings.WarehouseId },
            { $"{DatabricksSqlStatementOptions.DatabricksOptions}:MaxBufferedChunks", "15" },
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient();
        services.AddDatabricksSqlStatementExecution(configuration.GetSection(DatabricksSqlStatementOptions.DatabricksOptions));
        return services;
    }
}
