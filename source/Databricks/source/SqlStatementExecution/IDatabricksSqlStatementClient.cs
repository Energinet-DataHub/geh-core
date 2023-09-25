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

using System.Collections.Generic;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

/// <summary>
/// This interface is used to execute SQL statements against Databricks.
/// </summary>
public interface IDatabricksSqlStatementClient
{
    /// <summary>
    /// Get all the rows of a SQL query in as an asynchronous data stream.
    /// </summary>
    IAsyncEnumerable<SqlResultRow> ExecuteAsync(string sqlStatement);
}