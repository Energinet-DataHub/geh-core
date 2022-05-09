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

using System;
using System.Collections.Generic;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Database
{
    public class SqlServerConnectionStringProviderTests
    {
        private const string ValidConnectionString =
            "Data Source=myServerAddress;Database=myDataBase;Trusted_Connection=True;";

        private const string DatabasePrefix = "MyPrefix";
        private readonly string _databaseName = Guid.NewGuid().ToString("N");

        [Fact]
        public void When_EnvironmentVariableIsNotPresent_Then_LocalDb_IsSelected()
        {
            var runtime = new FlexibleRuntimeEnvironment(new Dictionary<string, string?>());
            var sut = new SqlServerConnectionStringProvider(runtime);

            var result = sut.BuildConnectionStringForDatabaseWithPrefix(DatabasePrefix);

            var builder = new SqlConnectionStringBuilder(result);

            builder.DataSource.Should().StartWith("(LocalDB)");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void When_EnvironmentVariableIsPresent_With_Empty_string_Then_LocalDB_Is_Selected(string? environmentValue)
        {
            var runtime = new FlexibleRuntimeEnvironment(CreateEnvironmentDictionary(environmentValue));
            var sut = new SqlServerConnectionStringProvider(runtime);

            var result = sut.BuildConnectionStringForDatabaseWithPrefix(DatabasePrefix);

            var builder = new SqlConnectionStringBuilder(result);

            builder.DataSource.Should().StartWith("(LocalDB)");
        }

        [Fact]
        public void When_EnvironmentVariableIsPresent_With_Value_Then_It_is_Selected()
        {
            var runtime = new FlexibleRuntimeEnvironment(CreateEnvironmentDictionary(ValidConnectionString));
            var sut = new SqlServerConnectionStringProvider(runtime);

            var result = sut.BuildConnectionStringForDatabaseWithPrefix(DatabasePrefix);

            var builder = new SqlConnectionStringBuilder(result);

            builder.DataSource.Should().StartWith("myServerAddress");
        }

        [Theory]
        [MemberData(nameof(GetRuntimes))]
        public void When_ConnectionString_Is_Created_Then_Initial_Catalog_Starts_With_Prefix(RuntimeEnvironment runtime)
        {
            var sut = new SqlServerConnectionStringProvider(runtime);

            var result = sut.BuildConnectionStringForDatabaseWithPrefix(DatabasePrefix);

            var builder = new SqlConnectionStringBuilder(result);

            builder.InitialCatalog.Should().StartWith(DatabasePrefix);
        }

        [Theory]
        [MemberData(nameof(GetRuntimes))]
        public void When_ConnectionString_Is_Created_With_DatabaseName_Then_Initial_Catalog_Is_DatabaseName(RuntimeEnvironment runtime)
        {
            var sut = new SqlServerConnectionStringProvider(runtime);

            var result = sut.BuildConnectionStringForDatabaseName(_databaseName);
            var builder = new SqlConnectionStringBuilder(result);

            builder.InitialCatalog.Should().Be(_databaseName);
        }

        [Fact]
        public void When_Invalid_ConnectionString_Is_Set_In_Environment_Variable_Then_An_ArgumentException_Is_Thrown()
        {
            var runtime = new FlexibleRuntimeEnvironment(CreateEnvironmentDictionary(Guid.Empty.ToString("N")));
            var sut = new SqlServerConnectionStringProvider(runtime);

            Assert.Throws<ArgumentException>(() => sut.BuildConnectionStringForDatabaseWithPrefix(DatabasePrefix));
        }

        public static IEnumerable<object[]> GetRuntimes()
        {
            yield return new object[] { new FlexibleRuntimeEnvironment(new Dictionary<string, string?>()) }; // no environment variable set
            yield return new object[] { new FlexibleRuntimeEnvironment(CreateEnvironmentDictionary(string.Empty)) }; // variable set with no value
            yield return new object[] { new FlexibleRuntimeEnvironment(CreateEnvironmentDictionary(null)) }; // variable set with null value
            yield return new object[] { new FlexibleRuntimeEnvironment(CreateEnvironmentDictionary(ValidConnectionString)) }; // environment with valid connection string override
        }

        private static Dictionary<string, string?> CreateEnvironmentDictionary(string? environmentValue)
        {
            var dictionary = new Dictionary<string, string?>
                { { nameof(FlexibleRuntimeEnvironment.TestCommonConnectionString), environmentValue } };
            return dictionary;
        }

        private class FlexibleRuntimeEnvironment : RuntimeEnvironment
        {
            private readonly Dictionary<string, string?> _environment;

            public FlexibleRuntimeEnvironment(Dictionary<string, string?> environment)
            {
                _environment = environment;
            }

            protected override string? GetEnvironmentVariable(string variableName)
                => _environment.GetValueOrDefault(variableName);
        }
    }
}
