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

/// <summary>
/// Produces 1.000.000 rows
/// </summary>
public class AtLeast10SecStatement : DatabricksStatement
{
    public AtLeast10SecStatement()
    {
        HelperId = Guid.NewGuid();
    }

    public Guid HelperId { get; private set; }

    protected internal override string GetSqlStatement()
    {
        return $"SELECT concat_ws('-', M.id, N.id, random()) as ID FROM range(1000000) AS M, range(1000000) AS N; --HelperId:{HelperId}";
    }
}
