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

using System.Diagnostics;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Fixtures;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using FluentAssertions;
using Xunit.Abstractions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client;

public class ApacheArrowArrayTests(DatabricksSqlWarehouseFixture databricksSqlWarehouseFixture) : IClassFixture<DatabricksSqlWarehouseFixture>
{
    [Fact]
    public async Task A_Result_With_StringArray_Is_Correctly_Mapped_When_An_Odd_Number_Of_Entries_Is_Present()
    {
        // Arrange
        var client = databricksSqlWarehouseFixture.CreateSqlStatementClient();
        var stmt = DatabricksStatement.FromRawSql(@"
SELECT * FROM VALUES
('Centrum', array('a', 'b', 'c')),
('Zen', array('d', 'e')),
('Rum', array('f')) as data(name, ts)").Build();

        var query = client.ExecuteStatementAsync(stmt);
        var result = await query.ToListAsync();

        result.Count.Should().Be(3);
        Assert.True(CheckRow(result[0], new[] { "a", "b", "c" }));
        Assert.True(CheckRow(result[1], new[] { "d", "e" }));
        Assert.True(CheckRow(result[2], new[] { "f" }));
    }

    private static bool CheckRow(dynamic row, string[] expectedData)
    {
        var ts = row.ts as object[];
        return ts?.SequenceEqual(expectedData) ?? false;
    }
}
