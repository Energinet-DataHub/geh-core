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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public sealed class DatabricksStatementBuilder
{
    private readonly string _rawSql;
    private readonly List<QueryParameter> _queryParameters = new();

    internal DatabricksStatementBuilder(string rawSql)
    {
        _rawSql = rawSql;
    }

    /// <summary>
    /// Add a <see cref="string"/> parameter to the SQL statement
    /// </summary>
    /// <param name="name">Name of parameter</param>
    /// <param name="value">Value for the parameter</param>
    public DatabricksStatementBuilder WithParameter(string name, string? value)
    {
        _queryParameters.Add(QueryParameter.Create(name, value));
        return this;
    }

    /// <summary>
    /// Add a <see cref="int"/> parameter to the SQL statement
    /// </summary>
    /// <param name="name">Name of parameter</param>
    /// <param name="value">Value for the parameter</param>
    public DatabricksStatementBuilder WithParameter(string name, int? value)
    {
        _queryParameters.Add(QueryParameter.Create(name, value));
        return this;
    }

    /// <summary>
    /// Add a <see cref="DateTimeOffset"/> parameter to the SQL statement
    /// </summary>
    /// <param name="name">Name of parameter</param>
    /// <param name="value">Value for the parameter</param>
    public DatabricksStatementBuilder WithParameter(string name, DateTimeOffset? value)
    {
        _queryParameters.Add(QueryParameter.Create(name, value));
        return this;
    }

    /// <summary>
    /// Add a <see cref="bool"/> parameter to the SQL statement
    /// </summary>
    /// <param name="name">Name of parameter</param>
    /// <param name="value">Value for the parameter</param>
    public DatabricksStatementBuilder WithParameter(string name, bool? value)
    {
        _queryParameters.Add(QueryParameter.Create(name, value));
        return this;
    }

    /// <summary>
    /// Add a <see cref="QueryParameter"/> parameter to the SQL statement
    /// </summary>
    /// <param name="queryParameter">Query parameter</param>
    public DatabricksStatementBuilder WithParameter(QueryParameter queryParameter)
    {
        _queryParameters.Add(queryParameter);
        return this;
    }

    /// <summary>
    /// Build the <see cref="DatabricksStatement"/>
    /// </summary>
    /// <returns>A <see cref="DatabricksStatement"/> with SQL and optional parameters</returns>
    public DatabricksStatement Build()
    {
        return new RawSqlStatement(_rawSql, _queryParameters.AsReadOnly());
    }
}
