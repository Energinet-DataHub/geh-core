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

using System.Collections.ObjectModel;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

internal class RawSqlStatement : DatabricksStatement
{
    private readonly string _rawSql;
    private readonly ReadOnlyCollection<QueryParameter> _sqlParameters;

    internal RawSqlStatement(string rawSql, ReadOnlyCollection<QueryParameter> sqlParameters)
    {
        _rawSql = rawSql;
        _sqlParameters = sqlParameters;
    }

    protected internal override string GetSqlStatement()
    {
        return _rawSql;
    }

    protected internal override IReadOnlyCollection<QueryParameter> GetParameters()
    {
        return _sqlParameters;
    }
}
