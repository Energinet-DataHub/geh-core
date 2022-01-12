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
using System.Data.SqlClient;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Database
{
    public class SqlServerConnectionStringBuilderTests
    {
        public class LocalDb
        {
            [Fact]
            public void When_DatabaseNameIsSupplied_Then_ItIsPartOfReturnedConnectionString()
            {
                // Arrange
                var builder = SqlServerConnectionStringBuilder.MSSQLLocalDb;
                var databaseName = Guid.NewGuid().ToString("N");

                // Act
                var connectionString = new SqlConnectionStringBuilder(builder.BuildConnectionString(databaseName));

                // Assert
                connectionString.InitialCatalog.Should().Be(databaseName);
                connectionString.IntegratedSecurity.Should().BeTrue();
            }

            [Fact]
            public void When_ConnectionStringIsBuilt_Then_TimeoutIs3Seconds()
            {
                var builder = SqlServerConnectionStringBuilder.MSSQLLocalDb;
                var databaseName = Guid.NewGuid().ToString("N");

                var connectionString = new SqlConnectionStringBuilder(builder.BuildConnectionString(databaseName));

                connectionString.ConnectTimeout.Should().Be(3);
            }

            [Fact]
            public void When_ConnectionStringIsBuilt_Then_DataSourceIsForLocalDb()
            {
                // Arrange
                var builder = SqlServerConnectionStringBuilder.MSSQLLocalDb;
                const string dataSource = "(LocalDB)\\MSSQLLocalDB";

                // Act
                var connectionString = new SqlConnectionStringBuilder(builder.BuildConnectionString(Guid.NewGuid().ToString("N")));

                // Assert
                connectionString.DataSource.Should().Be(dataSource);
            }

            [Fact]
            public void When_ConnectionStringIsBuilt_Then_IntegratedSecurityIsTrue()
            {
                // Arrange
                var builder = SqlServerConnectionStringBuilder.MSSQLLocalDb;

                // Act
                var connectionString = new SqlConnectionStringBuilder(builder.BuildConnectionString(Guid.NewGuid().ToString("N")));

                // Assert
                connectionString.IntegratedSecurity.Should().BeTrue();
            }
        }

        public class SqlServerUsernamePassword
        {
            [Fact]
            public void When_HostIsSupplied_Then_ItIsPartOfTheConnectionString()
            {
                var parameters = GetParameters();
                var builder = new SqlServerUsernamePasswordBuilder(parameters.Host, parameters.Username, parameters.Password);

                var connectionString =
                    new SqlConnectionStringBuilder(builder.BuildConnectionString(parameters.DatabaseName));

                connectionString.DataSource.Should().Be(parameters.Host);
            }

            [Fact]
            public void When_CredentialsIsSupplied_Then_ItIsPresent()
            {
                var opt = GetParameters();
                var builder = new SqlServerUsernamePasswordBuilder(opt.Host, opt.Username, opt.Password);

                var connectionString = new SqlConnectionStringBuilder(builder.BuildConnectionString(opt.DatabaseName));

                connectionString.UserID.Should().Be(opt.Username);
                connectionString.Password.Should().Be(opt.Password);
            }

            [Fact]
            public void When_DatabaseNameIsSupplied_Then_ItIsPartOfTheConnectionString()
            {
                var opt = GetParameters();
                var builder = new SqlServerUsernamePasswordBuilder(opt.Host, opt.Username, opt.Password);

                var connectionString = new SqlConnectionStringBuilder(builder.BuildConnectionString(opt.DatabaseName));

                connectionString.InitialCatalog.Should().Be(opt.DatabaseName);
            }

            private static (string Host, string Username, string Password, string DatabaseName) GetParameters()
                =>
                (Guid.NewGuid().ToString("N"),
                 Guid.NewGuid().ToString("N"),
                 Guid.NewGuid().ToString("N"),
                 Guid.NewGuid().ToString("N"));
        }
    }
}
