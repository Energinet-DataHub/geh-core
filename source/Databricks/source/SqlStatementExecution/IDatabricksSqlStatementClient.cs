﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Collections.Generic;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

/// <summary>
/// This interface is used to execute SQL statements against Databricks.
/// </summary>
public interface IDatabricksSqlStatementClient
{
    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results.
    /// </summary>
    /// <param name="sqlStatement">The SQL query to be executed, with placeholders for parameters. </param>
    /// <param name="sqlStatementParameters">A list of <see cref="SqlStatementParameter"/> objects representing parameters
    /// to be used in the query, preventing SQL injection vulnerabilities.</param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="SqlResultRow"/> representing the result set of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="sqlStatement"/> should contain Parameter Markers in the form of ':parameterName', that has corresponding
    /// <see cref="SqlStatementParameter"/> objects in the <paramref name="sqlStatementParameters"/> list.
    ///
    /// Optionally, a normal SQL query can be executed by passing an empty list of <see cref="SqlStatementParameter"/> objects.
    /// But it is recommended to always use Parameter Markers to protect against SQL injection attacks.
    /// </remarks>
    IAsyncEnumerable<SqlResultRow> ExecuteAsync(
        string sqlStatement,
        List<SqlStatementParameter> sqlStatementParameters);
}
