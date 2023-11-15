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

public class ObjectCreationTests : IClassFixture<DatabricksSqlWarehouseFixture>
{
    private readonly DatabricksSqlWarehouseFixture _sqlWarehouseFixture;

    public ObjectCreationTests(DatabricksSqlWarehouseFixture sqlWarehouseFixture)
    {
        _sqlWarehouseFixture = sqlWarehouseFixture;
    }

    [Fact]
    public async Task CanMapToRecord()
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();
        var statement = DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age)")
            .Build();

        // Act
        var result = client.ExecuteStatementAsync<Person>(statement);
        var persons = await result.ToListAsync();

        // Assert
        persons.Should().Contain(new Person("John A", 18));
    }

    public record Person(
        [property: ArrowField("name", 1)] string Name,
        [property: ArrowField("age", 2)] int Age);
}
