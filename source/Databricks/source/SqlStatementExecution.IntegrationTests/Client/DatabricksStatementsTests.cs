﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client.Statements;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client;

public class DatabricksStatementsTests : IClassFixture<DatabricksSqlWarehouseFixture>
{
    private readonly DatabricksSqlWarehouseFixture _sqlWarehouseFixture;

    public DatabricksStatementsTests(DatabricksSqlWarehouseFixture sqlWarehouseFixture)
    {
        _sqlWarehouseFixture = sqlWarehouseFixture;
    }

    [Theory]
    [MemberData(nameof(GetFormats))]
    public async Task ExecuteStatementAsync_WhenQueryingDynamic_MustReturnOneMillionRows(Format format)
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = new OneMillionRows();

        // Act
        var result = client.ExecuteStatementAsync(statement, format);
        var rowCount = await result.CountAsync();

        // Assert
        rowCount.Should().Be(1000000);
    }

    public static IEnumerable<object[]> GetFormats()
    {
        yield return new object[] { Format.ApacheArrow };
        yield return new object[] { Format.JsonArray };
    }
}