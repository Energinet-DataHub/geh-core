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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Client.Statements;

public class QueryPerson : DatabricksStatement
{
    private readonly string _personName;

    public QueryPerson(string personName)
    {
        _personName = personName;
   }

    protected internal override string GetSqlStatement()
    {
        return @"SELECT * FROM VALUES
              ('Zen Hui', 25),
              ('Anil B' , 18),
              ('Shone S', 16),
              ('Mike A' , 25),
              ('John A' , 18),
              ('Jack N' , 16) AS data(name, age)
                WHERE data.name = :personName";
    }

    protected internal override IReadOnlyCollection<QueryParameter> GetParameters()
    {
        return new[] { QueryParameter.Create("personName", _personName) };
    }
}
