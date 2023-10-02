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
using System.Text.Json;
using System.Text.Json.Nodes;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;

public class SqlChunkDataResponseParser : ISqlChunkDataResponseParser
{
    public IAsyncEnumerable<string[]?> ParseAsync(Stream jsonResponse, string[] columnNames)
    {
        return JsonSerializer.DeserializeAsyncEnumerable<string[]>(jsonResponse);

        /*await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<string[]>(jsonResponse))
        {
            yield return item;
        }*/

        /*var jsonArray = JsonSerializer.DeserializeAsyncEnumerable<JsonArray>(jsonResponse);

        // var jsonArray = JsonSerializer.Deserialize<JsonArray>(jsonResponse);
        /*var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonArray = JsonConvert.DeserializeObject<JArray>(jsonResponse, settings) ??
                         throw new InvalidOperationException();#1#

        var rows = GetDataArray(jsonArray);
        return new TableChunk(columnNames, rows);*/
    }

    private static List<string[]> GetDataArray(IAsyncEnumerable<JsonArray?> jsonArray)
    {
        var dataArray = jsonArray.GetAsyncEnumerator().Current.Deserialize<List<string[]>>() ??
                        throw new DatabricksSqlException("Unable to retrieve 'data_array' from the response");
        return dataArray;
    }
}
