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

using System;
using System.Linq;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;

public class SqlStatusResponseParser : ISqlStatusResponseParser
{
    private readonly ILogger<SqlStatusResponseParser> _logger;
    private readonly ISqlChunkResponseParser _chunkParser;

    public SqlStatusResponseParser(
        ILogger<SqlStatusResponseParser> logger,
        ISqlChunkResponseParser chunkParser)
    {
        _logger = logger;
        _chunkParser = chunkParser;
    }

    public SqlResponse Parse(string jsonResponse)
    {
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        try
        {
            var statementId = GetStatementId(jsonObject);
            var state = GetState(jsonObject);
            switch (state)
            {
                case "PENDING":
                    return SqlResponse.CreateAsPending(statementId);
                case "RUNNING":
                    return SqlResponse.CreateAsRunning(statementId);
                case "CLOSED":
                    return SqlResponse.CreateAsClosed(statementId);
                case "CANCELED":
                    return SqlResponse.CreateAsCancelled(statementId);
                case "FAILED":
                    return SqlResponse.CreateAsFailed(statementId);
                case "SUCCEEDED":
                    var columnNames = GetColumnNames(jsonObject);
                    var chunk = _chunkParser.Parse(GetChunk(jsonObject));
                    return SqlResponse.CreateAsSucceeded(statementId, columnNames, chunk);
                default:
                    throw new DatabricksSqlException($@"Databricks SQL statement execution failed. State: {state}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Databricks SQL statement execution failed. Response {JsonResponse}", jsonResponse);
            throw;
        }
    }

    private static Guid GetStatementId(JObject responseJsonObject)
    {
        return responseJsonObject["statement_id"]?.ToObject<Guid>() ?? throw new InvalidOperationException("Unable to retrieve 'statement_id' from the responseJsonObject");
    }

    private static string GetState(JObject responseJsonObject)
    {
        return responseJsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
    }

    private static string[] GetColumnNames(JObject responseJsonObject)
    {
        var columnNames = responseJsonObject["manifest"]?["schema"]?["columns"]?.Select(x => x["name"]?.ToString()).ToArray() ??
                          throw new DatabricksSqlException("Unable to retrieve 'columns' from the responseJsonObject.");
        return columnNames!;
    }

    private static JToken GetChunk(JObject responseJsonObject)
    {
        return responseJsonObject["result"]!;
    }
}
