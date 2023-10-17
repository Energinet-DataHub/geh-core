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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Client;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Constants;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.ResponseParsers;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Fixtures;

public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    public DatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksOptionsMock = CreateDatabricksOptionsMock(integrationTestConfiguration.DatabricksSettings);
        DatabricksSchemaManager = new DatabricksSchemaManager(DatabricksOptionsMock.Object.Value, "wholesale");
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    public Mock<IOptions<DatabricksSqlStatementOptions>> DatabricksOptionsMock { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public DatabricksSqlStatementClient CreateSqlStatementClient(
        DatabricksSqlStatementOptions databricksOptions,
        Mock<IHttpClientFactory> httpClientFactory,
        Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        Mock<ILogger<DatabricksSqlStatementClient>> loggerMock2)
    {
        var databricksSqlChunkResponseParser = new SqlChunkResponseParser();

        httpClientFactory.Setup(f => f.CreateClient(HttpClientNameConstants.Databricks))
            .Returns(HttpClientFactory.CreateHttpClient(databricksOptions));

        var sqlStatementClient = new DatabricksSqlStatementClient(
            httpClientFactory.Object,
            DatabricksOptionsMock.Object,
            new SqlResponseParser(
                new SqlStatusResponseParser(loggerMock.Object, databricksSqlChunkResponseParser),
                databricksSqlChunkResponseParser,
                new SqlChunkDataResponseParser()),
            loggerMock2.Object);
        return sqlStatementClient;
    }

    private static Mock<IOptions<DatabricksSqlStatementOptions>> CreateDatabricksOptionsMock(DatabricksSettings databricksSettings)
    {
        var databricksOptionsMock = new Mock<IOptions<DatabricksSqlStatementOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new DatabricksSqlStatementOptions
            {
                WorkspaceUrl = databricksSettings.WorkspaceUrl,
                WorkspaceToken = databricksSettings.WorkspaceAccessToken,
                WarehouseId = databricksSettings.WarehouseId,
            });

        return databricksOptionsMock;
    }
}
