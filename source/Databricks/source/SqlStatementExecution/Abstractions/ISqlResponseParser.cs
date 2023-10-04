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
/// This interface is used to parse the response from the Databricks SQL API.
/// </summary>
public interface ISqlResponseParser
{
    /// <summary>
    /// Parse the response Status from the Databricks SQL API.
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <returns>Returns <see cref="SqlResponse"/></returns>
    SqlResponse ParseStatusResponse(string jsonResponse);

    /// <summary>
    /// Parse the response Chunk from the Databricks SQL API.
    ///
    /// <br></br>Example of chunk response:
    /// <br></br>{
    /// <br></br>"row_count": 41667,
    /// <br></br>"byte_count": 1,
    /// <br></br>"next_chunk_index": 1,
    /// <br></br>"next_chunk_internal_link": "a-line",
    /// <br></br>"external_link": "a-link",
    /// <br></br>"expiration": "2023-07-04T00:00:00.0000"
    /// <br></br>}
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <returns>Returns <see cref="SqlChunkResponse"/></returns>
    SqlChunkResponse ParseChunkResponse(string jsonResponse);

    /// <summary>
    /// Parse the response Chunk Data from the Databricks SQL API.
    ///
    /// <br></br>Example of chunk data response:
    /// <br></br>[["0","some value"], ["1","some value"]]
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <param name="columnNames"></param>
    /// <returns>Returns <see cref="TableChunk"/></returns>
    TableChunk ParseChunkDataResponse(string jsonResponse, string[] columnNames);

    /// <summary>
    /// Parse the response Chunk Data from the Databricks SQL API.
    ///
    /// <br></br>Example of chunk data response:
    /// <br></br>[["0","some value"], ["1","some value"]]
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <param name="columnNames"></param>
    /// <returns>Returns <see cref="TableChunk"/></returns>
    IAsyncEnumerable<string[]> ParseChunkDataResponseAsync(Stream jsonResponse, string[] columnNames);
}
