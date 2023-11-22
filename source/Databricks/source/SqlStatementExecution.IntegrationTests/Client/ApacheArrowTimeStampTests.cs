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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client;

public class ApacheArrowTimeStampTests : IClassFixture<DatabricksSqlWarehouseFixture>
{
    private readonly DatabricksSqlWarehouseFixture _sqlWarehouseFixture;

    public ApacheArrowTimeStampTests(DatabricksSqlWarehouseFixture sqlWarehouseFixture)
    {
        _sqlWarehouseFixture = sqlWarehouseFixture;
    }

    [Fact]
    public async Task CanMapTimestampToDate()
    {
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = DatabricksStatement.FromRawSql(
            @"SELECT TIMESTAMP'2021-7-1T8:43:28.123' AS bar").Build();

        // Act
        var result = client.ExecuteStatementAsync(statement, Format.ApacheArrow);
        var row = await result.FirstAsync();

        DateTimeOffset expected = new(2021, 7, 1, 8, 43, 28, 123, TimeSpan.Zero);
        DateTimeOffset bar = row.bar;

        bar.Should().Be(expected);
    }
}
