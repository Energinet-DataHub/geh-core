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

    [Fact]
    public async Task CanHandleArrayTypes()
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = DatabricksStatement.FromRawSql(
            @"SELECT a, b FROM VALUES
                ('one', array(0, 1)),
                ('two', array(2, 3)) AS data(a, b);").Build();

        // Act
        var result = client.ExecuteStatementAsync(statement, Format.JsonArray);
        var row = await result.FirstAsync();
        var values = JsonSerializer
                            .Deserialize<string[]>((string)row.b)?
                            .Select(s => Convert.ToInt32(s))
                            .ToArray();

        // Assert
        values.Should().NotBeNull();
        values.Should().BeEquivalentTo(new[] { 0, 1 });
    }

    [Fact]
    public async Task ExecuteStatement_FromRawSqlWithParameters_ShouldReturnRows()
    {
        // Arrange
        const int expectedRows = 2;
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age)
              WHERE data.age = :_age;")
            .WithParameter("_age", 25)
            .Build();

        // Act
        var result = client.ExecuteStatementAsync(statement);
        var rowCount = await result.CountAsync();

        // Assert
        rowCount.Should().Be(expectedRows);
    }

    [Fact]
    public async Task ExecuteStatement_FromRawSql_ShouldReturnRows()
    {
        // Arrange
        const int expectedRows = 6;
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age);")
            .Build();

        // Act
        var result = client.ExecuteStatementAsync(statement);
        var rowCount = await result.CountAsync();

        // Assert
        rowCount.Should().Be(expectedRows);
    }

    [Fact]
    public async Task ExecuteStatementAsync_WhenQueryingWithParameters_MustIncludeParameterTypeInRequest()
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = new LimitRows(10);

        // Act
        var result = client.ExecuteStatementAsync(statement, Format.ApacheArrow);
        var rowCount = await result.CountAsync();

        // Assert
        rowCount.Should().Be(10);
    }

    [Theory]
    [InlineData("Mike A", 1)]
    [InlineData("Sheldon Cooper", 0)]
    public async Task ExecuteStatementAsync_WhenQueryingWithStringParameter_MustReturnRows(string searchFor, int expectedRows)
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = new QueryPerson(searchFor);

        // Act
        var result = client.ExecuteStatementAsync(statement, Format.ApacheArrow);
        var rowCount = await result.CountAsync();

        // Assert
        rowCount.Should().Be(expectedRows);
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
