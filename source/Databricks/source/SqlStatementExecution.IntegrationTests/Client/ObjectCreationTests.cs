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
using Xunit.Abstractions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client;

public class ObjectCreationTests : IClassFixture<DatabricksSqlWarehouseFixture>
{
    private readonly DatabricksSqlWarehouseFixture _sqlWarehouseFixture;
    private readonly ITestOutputHelper _writer;

    private static DatabricksStatement PersonsStatement => DatabricksStatement.FromRawSql(@"
SELECT * FROM (
    SELECT
        name, age,
        concat('email_', name, '@example.com') as email,
        concat('address_', cast(age as string)) as address,
        concat('city_', cast(age as string)) as city,
        concat('state_', cast(age % 50 as string)) as state,
        concat('zip_', cast(age * 1000 as string)) as zip,
        concat('phone_', cast(age * 10000 as string)) as phone,
        date_add(current_date(), age) as birth_date,
        age * 1000 as salary,
        concat('department_', cast(age % 5 as string)) as department,
        concat('position_', cast(age % 10 as string)) as position,
        age % 2 = 0 as is_active,
        uuid() as id,
        concat('username_', lower(replace(name, ' ', '_'))) as username,
        sha2(name, 256) as password_hash,
        age * 12 as months_employed,
        age > 20 as is_senior,
        'Employee' as type,
        current_timestamp() as created_at,
        date_add(current_timestamp(), 365) as contract_end,
        concat('region_', cast(age % 4 as string)) as region,
        concat('team_', cast(age % 7 as string)) as team,
        age * 1.5 as performance_score,
        concat('skill_', cast(age % 6 as string)) as primary_skill,
        concat('level_', cast(age % 5 as string)) as skill_level,
        concat('certificate_', cast(age % 3 as string)) as certification,
        current_date() as last_promotion_date,
        age / 10.0 as years_of_experience,
        age * 2 as vacation_days,
        concat('project_', cast(age % 8 as string)) as current_project,
        cast(age > 18 as string) as legal_status,
        concat('emergency_contact_', cast(age as string)) as emergency_contact,
        concat('office_', cast(age % 10 as string)) as office_location,
        age * 0.25 as bonus_factor,
        concat('manager_', cast(age % 5 as string)) as manager,
        concat('device_', cast(age % 4 as string)) as assigned_device,
        'English' as language,
        concat('Social_', cast(age % 3 as string)) as social_security,
        age % 5 as priority_level,
        concat('Comment_', cast(age as string)) as notes
    FROM (
        VALUES
            ('Zen Hui', 25),
            ('Anil B', 18),
            ('Shone S', 16),
            ('Mike A', 25),
            ('John A', 18),
            ('Jack N', 16)
    ) AS base_data(name, age)
    JOIN (
        SELECT id FROM RANGE(1, 1000001)
    ) AS multiplier
)
").Build();

    private static DatabricksStatement NullStatement => DatabricksStatement.FromRawSql(@"SELECT * FROM VALUES
('Jack N' , 16, 'Developer'),
('Null N' , 27, NULL) AS data(name, age, title)")
        .Build();

    public ObjectCreationTests(DatabricksSqlWarehouseFixture sqlWarehouseFixture, ITestOutputHelper writer)
    {
        _sqlWarehouseFixture = sqlWarehouseFixture;
        _writer = writer;
        DebugInfo.ResetCounters();
        DebugInfo.ResetMeasurements();
    }

    [Theory(Skip = "Way to heavy")]
    [MemberData(nameof(ReflectionTypes))]
    public async Task CanMapToRecord(ReflectionStrategy reflectionStrategy)
    {
        // Arrange
        var client = _sqlWarehouseFixture.CreateSqlStatementClient();

        for (var i = 0; i < 1; i++)
        {
            // Act
            var result = client.ExecuteStatementAsync<Person>(PersonsStatement, reflectionStrategy);
            var persons = await result.ToListAsync();

            // Assert
            //persons.Should().Contain(new Person("John A", 18));
        }

        DebugInfo.PrintCounters(_writer.WriteLine);
        DebugInfo.PrintMeasurements(_writer.WriteLine);
    }

    [Theory(Skip = "Way to heavy")]
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

    [Theory(Skip = "Way to heavy")]
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

    // public record Person(
    //     [property: ArrowField("name", 1)] string Name,
    //     [property: ArrowField("age", 2)] int Age);
    public record Person(
    [property: ArrowField("name", 1)] string Name,
    [property: ArrowField("age", 2)] int Age,
    [property: ArrowField("email", 3)] string Email,
    [property: ArrowField("address", 4)] string Address,
    [property: ArrowField("city", 5)] string City,
    [property: ArrowField("state", 6)] string State,
    [property: ArrowField("zip", 7)] string Zip,
    [property: ArrowField("phone", 8)] string Phone,
    [property: ArrowField("birth_date", 9)] int BirthDate,
    [property: ArrowField("salary", 10)] int Salary,
    [property: ArrowField("department", 11)] string Department,
    [property: ArrowField("position", 12)] string Position,
    [property: ArrowField("is_active", 13)] bool IsActive,
    [property: ArrowField("id", 14)] string Id,
    [property: ArrowField("username", 15)] string Username,
    [property: ArrowField("password_hash", 16)] string PasswordHash,
    [property: ArrowField("months_employed", 17)] int MonthsEmployed,
    [property: ArrowField("is_senior", 18)] bool IsSenior,
    [property: ArrowField("type", 19)] string Type,
    [property: ArrowField("created_at", 20)] DateTimeOffset CreatedAt,
    [property: ArrowField("contract_end", 21)] int ContractEnd,
    [property: ArrowField("region", 22)] string Region,
    [property: ArrowField("team", 23)] string Team,
    [property: ArrowField("performance_score", 24)] decimal PerformanceScore,
    [property: ArrowField("primary_skill", 25)] string PrimarySkill,
    [property: ArrowField("skill_level", 26)] string SkillLevel,
    [property: ArrowField("certification", 27)] string Certification,
    [property: ArrowField("last_promotion_date", 28)] int LastPromotionDate,
    [property: ArrowField("years_of_experience", 29)] decimal YearsOfExperience,
    [property: ArrowField("vacation_days", 30)] int VacationDays,
    [property: ArrowField("current_project", 31)] string CurrentProject,
    [property: ArrowField("legal_status", 32)] string LegalStatus,
    [property: ArrowField("emergency_contact", 33)] string EmergencyContact,
    [property: ArrowField("office_location", 34)] string OfficeLocation,
    [property: ArrowField("bonus_factor", 35)] decimal BonusFactor,
    [property: ArrowField("manager", 36)] string Manager,
    [property: ArrowField("assigned_device", 37)] string AssignedDevice,
    [property: ArrowField("language", 38)] string Language,
    [property: ArrowField("social_security", 39)] string SocialSecurity,
    [property: ArrowField("priority_level", 40)] int PriorityLevel,
    [property: ArrowField("notes", 41)] string Notes);

    public record PersonWithTitle(
        [property: ArrowField("name", 1)] string Name,
        [property: ArrowField("age", 2)] int Age,
        [property: ArrowField("title", 3)] string? Title);
}
