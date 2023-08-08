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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.AppSettings;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace SqlStatementExecution.IntegrationTests.Fixtures;

public class DatabricksSqlStatementApiFixture : IAsyncLifetime
{
    public DatabricksSqlStatementApiFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        DatabricksSchemaManager = new DatabricksSchemaManager(integrationTestConfiguration.DatabricksSettings, "wholesale");
        DatabricksOptionsMock = CreateDatabricksOptionsMock(DatabricksSchemaManager);
    }

    public DatabricksSchemaManager DatabricksSchemaManager { get; }

    public Mock<IOptions<DatabricksOptions>> DatabricksOptionsMock { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public SqlStatementClient CreateSqlStatementClient(Mock<ILogger<DatabricksSqlStatusResponseParser>> loggerMock, Mock<ILogger<SqlStatementClient>> loggerMock2)
    {
        var databricksSqlChunkResponseParser = new DatabricksSqlChunkResponseParser();
        var sqlStatementClient = new SqlStatementClient(
            new HttpClient(),
            DatabricksOptionsMock.Object,
            new DatabricksSqlResponseParser(
                new DatabricksSqlStatusResponseParser(loggerMock.Object, databricksSqlChunkResponseParser),
                databricksSqlChunkResponseParser,
                new DatabricksSqlChunkDataResponseParser()),
            loggerMock2.Object);
        return sqlStatementClient;
    }

    private static Mock<IOptions<DatabricksOptions>> CreateDatabricksOptionsMock(DatabricksSchemaManager databricksSchemaManager)
    {
        var databricksOptionsMock = new Mock<IOptions<DatabricksOptions>>();
        databricksOptionsMock
            .Setup(o => o.Value)
            .Returns(new DatabricksOptions
            {
                WorkspaceUrl = databricksSchemaManager.Settings.WorkspaceUrl,
                WorkspaceToken = databricksSchemaManager.Settings.WorkspaceAccessToken,
                WarehouseId = databricksSchemaManager.Settings.WarehouseId,
            });

        return databricksOptionsMock;
    }
}
