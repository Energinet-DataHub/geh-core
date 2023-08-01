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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

/// <summary>
/// This interface is used to parse the response from the Databricks SQL API.
/// </summary>
public interface IDatabricksSqlResponseParser
{
    /// <summary>
    /// Parse the response Status from the Databricks SQL API.
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <returns>Returns <see cref="DatabricksSqlResponse"/></returns>
    DatabricksSqlResponse ParseStatusResponse(string jsonResponse);

    /// <summary>
    /// Parse the response Chunk from the Databricks SQL API.
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <returns>Returns <see cref="DatabricksSqlChunkResponse"/></returns>
    DatabricksSqlChunkResponse ParseChunkResponse(string jsonResponse);

    /// <summary>
    /// Parse the response Chunk Data from the Databricks SQL API.
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <param name="columnNames"></param>
    /// <returns>Returns <see cref="TableChunk"/></returns>
    TableChunk ParseChunkDataResponse(string jsonResponse, string[] columnNames);
}
