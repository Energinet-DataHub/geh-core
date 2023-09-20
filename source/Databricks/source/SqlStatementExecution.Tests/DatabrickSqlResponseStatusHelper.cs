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

using Newtonsoft.Json;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests;

public static class DatabrickSqlResponseStatusHelper
{
    public static string CreateStatusResponse(string state)
    {
        var statement = new
        {
            statement_id = "01edef23-0d2c-10dd-879b-26b5e97b3796",
            status = new { state, },
        };
        return JsonConvert.SerializeObject(statement, Formatting.Indented);
    }
}
