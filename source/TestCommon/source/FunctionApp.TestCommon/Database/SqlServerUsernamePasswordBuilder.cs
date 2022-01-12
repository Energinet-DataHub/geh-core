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

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Database
{
    public class SqlServerUsernamePasswordBuilder : SqlServerConnectionStringBuilder
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;

        public SqlServerUsernamePasswordBuilder(string host, string username, string password)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public override string BuildConnectionString(string databaseName)
        {
            if (databaseName == null) throw new ArgumentNullException(nameof(databaseName));

            return $"Server={_host};Database={databaseName};User Id={_username};Password={_password};";
        }
    }
}
