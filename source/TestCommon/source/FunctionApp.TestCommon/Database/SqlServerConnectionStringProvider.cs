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
using Microsoft.Data.SqlClient;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Database
{
    public class SqlServerConnectionStringProvider
    {
        private readonly RuntimeEnvironment _environment;

        public SqlServerConnectionStringProvider(RuntimeEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// We create a unique database name to ensure tests using this fixture has their own database.
        /// </summary>
        public string BuildConnectionStringForDatabaseWithPrefix(string prefixForDatabaseName)
        {
            var databaseName = $"{prefixForDatabaseName}Database_{Guid.NewGuid()}";

            return BuildConnectionStringForDatabaseName(databaseName);
        }

        /// <summary>
        /// Build a connection string for a database name. Uses <see cref="RuntimeEnvironment.TestCommonConnectionString"/>.
        /// If null or empty, it will fallback to use localDb
        /// </summary>
        /// <param name="databaseName">Name of database</param>
        /// <returns>Connection string to a database</returns>
        public string BuildConnectionStringForDatabaseName(string databaseName)
        {
            return
                BuildConnectionStringFromEnvironmentVariable(_environment, databaseName) ??
                BuildConnectionStringForLocalDb(databaseName);
        }

        private static string BuildConnectionStringForLocalDb(string databaseName)
        {
            return $"Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;Database={databaseName};";
        }

        private static string? BuildConnectionStringFromEnvironmentVariable(RuntimeEnvironment environment, string databaseName)
        {
            var env = environment.TestCommonConnectionString;
            if (string.IsNullOrEmpty(env)) return null;

            var connectionStringBuilder = new SqlConnectionStringBuilder(env) { InitialCatalog = databaseName };

            return connectionStringBuilder.ConnectionString;
        }
    }
}
