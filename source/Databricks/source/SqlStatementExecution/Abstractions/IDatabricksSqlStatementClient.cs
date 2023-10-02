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

using System.Collections.Generic;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;

/// <summary>
/// This interface is used to execute SQL statements against Databricks SQL Statement Execution API.
/// </summary>
public interface IDatabricksSqlStatementClient
{
    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results.
    /// </summary>
    /// <param name="sqlStatement">The SQL query to be executed, with Parameter Markers for parameters. </param>
    /// <param name="sqlStatementParameters">[Optional] A list of <see cref="SqlStatementParameter"/> objects representing parameters
    ///     to be used in the query.</param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="SqlResultRow"/> representing the result set of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="sqlStatement"/> should contain Parameter Markers in the form of ':parameterName', that has corresponding
    /// <see cref="SqlStatementParameter"/> objects in the <paramref name="sqlStatementParameters"/> list.
    ///
    /// Optionally, to simply execute a SQL query without parameter markers, the optional <paramref name="sqlStatementParameters"/> parameter
    /// can be left empty. However, it is recommended to make use of parameter markers to protect against SQL injection attacks.
    /// </remarks>
    IAsyncEnumerable<string[]?> ExecuteAsync(
        string sqlStatement,
        List<SqlStatementParameter>? sqlStatementParameters);
}
