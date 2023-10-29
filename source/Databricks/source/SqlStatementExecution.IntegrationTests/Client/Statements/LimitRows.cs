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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client.Statements;

public class LimitRows : DatabricksStatement
{
    private readonly int _limit;

    public LimitRows(int limit)
    {
        _limit = limit;
    }

    protected internal override string GetSqlStatement()
    {
        return "SELECT concat_ws('-', M.id, N.id, random()) as ID FROM range(1000) AS M, range(1000) AS N LIMIT :limit";
    }

    protected internal override IReadOnlyCollection<QueryParameter> GetParameters()
    {
        return new[] { QueryParameter.Create("limit", _limit) };
    }
}
