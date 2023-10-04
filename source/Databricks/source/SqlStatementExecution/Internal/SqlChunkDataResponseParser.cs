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
using System.IO;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;

public class SqlChunkDataResponseParser : ISqlChunkDataResponseParser
{
    public TableChunk Parse(string jsonResponse, string[] columnNames)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonArray = JsonConvert.DeserializeObject<JArray>(jsonResponse, settings) ??
                        throw new InvalidOperationException();

        var rows = GetDataArray(jsonArray);
        return new TableChunk(columnNames, rows);
    }

    public IAsyncEnumerable<string[]> ParseAsync(Stream jsonStream, string[] columnNames)
    {
        var asyncEnumerable = JsonSerializer.DeserializeAsyncEnumerable<string[]>(jsonStream);

        if (asyncEnumerable == null)
        {
            throw new DatabricksSqlException("Unable to retrieve 'data_array' from the response");
        }

        return asyncEnumerable!;
    }

    private static List<string[]> GetDataArray(JArray jsonArray)
    {
        var dataArray = jsonArray.ToObject<List<string[]>>() ??
                        throw new DatabricksSqlException("Unable to retrieve 'data_array' from the response");
        return dataArray;
    }
}
