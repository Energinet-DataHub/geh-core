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

    private static DatabricksStatement PersonsStatement => DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age)")
        .Build();

    private static DatabricksStatement NullStatement => DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
('Jack N' , 16, 'Developer'),
('Null N' , 27, NULL) AS data(name, age, title)")
        .Build();

    public ObjectCreationTests(DatabricksSqlWarehouseFixture sqlWarehouseFixture)
    {
        _sqlWarehouseFixture = sqlWarehouseFixture;
    }

    [Theory]
    [MemberData(nameof(ReflectionTypes))]
    public async Task CanMapToRecord(ReflectionStrategy reflectionStrategy)
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();

        // Act
        var result = client.ExecuteStatementAsync<Person>(PersonsStatement, reflectionStrategy);
        var persons = await result.ToListAsync();

        // Assert
        persons.Should().Contain(new Person("John A", 18));
    }

    [Theory]
    [MemberData(nameof(ReflectionTypes))]
    public async Task GivenAClassWithMultipleConstructors_WhenConstructingObject_ThenExceptionIsThrown(ReflectionStrategy reflectionStrategy)
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();

        // Act
        var result = client.ExecuteStatementAsync<BadPerson>(PersonsStatement, reflectionStrategy);
        Func<Task> act = async () => await result.ToListAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [MemberData(nameof(ReflectionTypes))]
    public async Task GivenAClassWithTwoParameters_WhenOnlyOneIsMapped_ThenExceptionIsThrown(ReflectionStrategy reflectionStrategy)
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();

        // Act
        var result = client.ExecuteStatementAsync<ReallyBadPerson>(PersonsStatement, reflectionStrategy);
        Func<Task> act = async () => await result.ToListAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(ReflectionTypes))]
    public async Task AResponseWithNullValues_WhenMappedToANullableProperty_ThenTheObjectIsCreated(ReflectionStrategy reflectionStrategy)
    {
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();

        var result = client.ExecuteStatementAsync<PersonWithTitle>(NullStatement, reflectionStrategy);
        var persons = await result.ToListAsync();

        persons.Count.Should().Be(2);
    }

    public static IEnumerable<object[]> ReflectionTypes()
    {
        yield return [ReflectionStrategy.Default];
        yield return [ReflectionStrategy.Cache];
        yield return [ReflectionStrategy.Lambda];
    }

    public class ReallyBadPerson
    {
        public string Name { get; private set; }

        [ArrowField("age", 2)]
        public int Age { get; private set; }

        public ReallyBadPerson(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }

    public class BadPerson
    {
        public BadPerson()
            : this(string.Empty) { }

        public BadPerson(string name) => Name = name;

        public string Name { get; set; }
    }

    public record Person(
        [property: ArrowField("name", 1)] string Name,
        [property: ArrowField("age", 2)] int Age);

    public record PersonWithTitle(
        [property: ArrowField("name", 1)] string Name,
        [property: ArrowField("age", 2)] int Age,
        [property: ArrowField("title", 3)] string? Title);
}
