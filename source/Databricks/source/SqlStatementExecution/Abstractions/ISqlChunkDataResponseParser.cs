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
using System.IO;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;

/// <summary>
/// Parses the response from a Databricks SQL statement execution.
/// </summary>
public interface ISqlChunkDataResponseParser
{
    /// <summary>
    /// Parses the chunk data response from a Databricks SQL statement execution.
    ///
    /// The Chunk Data can vary in response depending on the type of statement executed.
    /// Therefore a list of column names is required to parse the response.
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <param name="columnNames"></param>
    /// <returns>Returns a <see cref="TableChunk"/></returns>
    IAsyncEnumerable<string[]> ParseAsync(Stream jsonResponse, string[] columnNames);
}
