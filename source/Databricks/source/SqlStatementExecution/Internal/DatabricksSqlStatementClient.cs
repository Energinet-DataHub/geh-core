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
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0
// https://learn.microsoft.com/en-gb/azure/databricks/sql/api/sql-execution-tutorial
public class DatabricksSqlStatementClient : IDatabricksSqlStatementClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly HttpClient _externalHttpClient;
    private readonly IOptions<DatabricksOptions> _options;
    private readonly IDatabricksSqlResponseParser _responseResponseParser;
    private readonly ILogger<DatabricksSqlStatementClient> _logger;

    public DatabricksSqlStatementClient(
        IHttpClientFactory httpClientFactory,
        IOptions<DatabricksOptions> options,
        IDatabricksSqlResponseParser responseResponseParser,
        ILogger<DatabricksSqlStatementClient> logger)
    {
        _options = options;
        _responseResponseParser = responseResponseParser;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
        _externalHttpClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
    }

    public async IAsyncEnumerable<SqlResultRow> ExecuteAsync(string sqlStatement)
    {
        _logger.LogDebug("Executing SQL statement: {Sql}", HttpUtility.HtmlEncode(sqlStatement));

        var response = await GetFirstChunkOrNullAsync(sqlStatement).ConfigureAwait(false);
        var columnNames = response.ColumnNames;
        var chunk = response.Chunk;
        var rowCount = 0;

        while (chunk != null)
        {
            if (chunk.ExternalLink == null)
            {
                break;
            }

            var data = await GetChunkDataAsync(chunk.ExternalLink, columnNames!).ConfigureAwait(false);

            for (var index = 0; index < data.Rows.Count; index++)
            {
                yield return new SqlResultRow(data, index);
                rowCount++;
            }

            if (chunk.NextChunkInternalLink == null)
            {
                break;
            }

            chunk = await GetChunkAsync(chunk.NextChunkInternalLink).ConfigureAwait(false);
        }

        _logger.LogDebug("SQL statement executed. Rows returned: {RowCount}", rowCount);
    }

    private async Task<DatabricksSqlResponse> GetFirstChunkOrNullAsync(string sqlStatement)
    {
        const int timeOutPerAttemptSeconds = 30;

        var requestObject = new
        {
            wait_timeout = $"{timeOutPerAttemptSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = _options.Value.WarehouseId,
            disposition = "EXTERNAL_LINKS", // Some results are larger than the maximum allowed 16MB limit, thus we need to use external links
        };
        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException($"Unable to get result from Databricks. HTTP status code: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var databricksSqlResponse = _responseResponseParser.ParseStatusResponse(jsonResponse);
        LogDatabricksSqlResponseState(databricksSqlResponse);

        var waitTime = 1000;
        while (databricksSqlResponse.State is DatabricksSqlResponseState.Pending or DatabricksSqlResponseState.Running)
        {
            if (waitTime > 600000)
            {
                throw new DatabricksSqlException($"Unable to get result from Databricks because the SQL statement execution didn't succeed. State: {databricksSqlResponse.State}");
            }

            waitTime *= 2;
            await Task.Delay(waitTime).ConfigureAwait(false);

            var path = $"{StatementsEndpointPath}/{databricksSqlResponse.StatementId}";
            var httpResponse = await _httpClient.GetAsync(path).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new DatabricksSqlException($"Unable to get result from Databricks. HTTP status code: {httpResponse.StatusCode}");
            }

            jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            databricksSqlResponse = _responseResponseParser.ParseStatusResponse(jsonResponse);
            LogDatabricksSqlResponseState(databricksSqlResponse);
        }

        if (databricksSqlResponse.State is not DatabricksSqlResponseState.Succeeded)
        {
            throw new DatabricksSqlException($"Unable to get result from Databricks because the SQL statement execution didn't succeed. State: {databricksSqlResponse.State}");
        }

        return databricksSqlResponse;
    }

    private async Task<DatabricksSqlChunkResponse> GetChunkAsync(string chunkLink)
    {
        var httpResponse = await _httpClient.GetAsync(chunkLink).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException($"Unable to get chunk from {chunkLink}. HTTP status code: {httpResponse.StatusCode}");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return _responseResponseParser.ParseChunkResponse(jsonResponse);
    }

    private async Task<TableChunk> GetChunkDataAsync(Uri? externalLink, string[] columnNames)
    {
        var httpResponse = await _externalHttpClient.GetAsync(externalLink).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException($"Unable to get chunk data from external link {externalLink}. HTTP status code: {httpResponse.StatusCode}");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return _responseResponseParser.ParseChunkDataResponse(jsonResponse, columnNames);
    }

    private void LogDatabricksSqlResponseState(DatabricksSqlResponse response)
    {
        _logger.LogDebug(
            "Databricks SQL response received with state: {State} for statement ID: {StatementId}",
            response.State,
            response.StatementId);
    }
}
