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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Constants;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Client;

/// <summary>
/// A client to execute SQL statements against Databricks.
///
/// This class has 2 HttpClients. The first one is used to execute the SQL statement
/// and the second one is used to get the data from the external link.
/// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0
/// https://learn.microsoft.com/en-gb/azure/databricks/sql/api/sql-execution-tutorial
/// </summary>
[Obsolete(message: "This interface is obsolete and will be removed in a future version. Please use DatabricksSqlWarehouseQueryExecutor instead.", error: false)]
public class DatabricksSqlStatementClient : IDatabricksSqlStatementClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly HttpClient _externalHttpClient;
    private readonly DatabricksSqlStatementOptions _options;
    private readonly ISqlResponseParser _responseResponseParser;
    private readonly ILogger<DatabricksSqlStatementClient> _logger;

    public DatabricksSqlStatementClient(
        IHttpClientFactory httpClientFactory,
        IOptions<DatabricksSqlStatementOptions> options,
        ISqlResponseParser responseResponseParser,
        ILogger<DatabricksSqlStatementClient> logger)
    {
        _options = options.Value;
        _responseResponseParser = responseResponseParser;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
        _externalHttpClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
    }

    public async IAsyncEnumerable<SqlResultRow> ExecuteAsync(
        string sqlStatement,
        List<SqlStatementParameter>? sqlStatementParameters = null)
    {
        sqlStatementParameters ??= new List<SqlStatementParameter>();
        LogDatabricksRequest(sqlStatement, sqlStatementParameters);

        var response = await GetFirstChunkOrNullAsync(sqlStatement, sqlStatementParameters).ConfigureAwait(false);
        var columnNames = response.ColumnNames;
        var chunk = response.Chunk;
        var rowCount = 0;

        while (chunk != null)
        {
            if (chunk.ExternalLink == null) break;

            var data = await GetChunkDataAsStringAsync(chunk.ExternalLink, columnNames!).ConfigureAwait(false);

            for (var index = 0; index < data.Rows.Count; index++)
            {
                yield return new SqlResultRow(data, index);
                rowCount++;
            }

            if (chunk.NextChunkInternalLink == null) break;

            chunk = await GetChunkAsync(chunk.NextChunkInternalLink).ConfigureAwait(false);
        }

        _logger.LogDebug("SQL statement executed. Rows returned: {RowCount}", rowCount);
    }

    public async IAsyncEnumerable<string[]> StreamAsync(
        string sqlStatement,
        List<SqlStatementParameter>? sqlStatementParameters = null)
    {
        sqlStatementParameters ??= new List<SqlStatementParameter>();
        LogDatabricksRequest(sqlStatement, sqlStatementParameters);

        var response = await GetFirstChunkOrNullAsync(sqlStatement, sqlStatementParameters).ConfigureAwait(false);

        var chunk = response.Chunk;
        var rowCount = 0;

        while (chunk != null)
        {
            if (chunk.ExternalLink == null) break;

            var data = await GetChunkDataAsStreamAsync(chunk.ExternalLink).ConfigureAwait(false);

            await foreach (var row in data)
            {
                yield return row;
                rowCount++;
            }

            if (chunk.NextChunkInternalLink == null) break;

            chunk = await GetChunkAsync(chunk.NextChunkInternalLink).ConfigureAwait(false);
        }

        _logger.LogDebug("SQL statement executed. Rows returned: {RowCount}", rowCount);
    }

    private async Task<SqlResponse> GetFirstChunkOrNullAsync(
        string sqlStatement, List<SqlStatementParameter> sqlStatementParameters)
    {
        var requestObject = new
        {
            wait_timeout = $"{_options.TimeoutInSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            parameters = sqlStatementParameters,
            warehouse_id = _options.WarehouseId,
            disposition = "EXTERNAL_LINKS", // Some results are larger than the maximum allowed 16MB limit, thus we need to use external links
        };
        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException(
                $"Unable to get response from Databricks. HTTP status code: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var databricksSqlResponse = _responseResponseParser.ParseStatusResponse(jsonResponse);
        LogDatabricksSqlResponseState(databricksSqlResponse);

        var waitTime = 1000;
        while (databricksSqlResponse.State is SqlResponseState.Pending or SqlResponseState.Running)
        {
            if (waitTime > 600000)
            {
                throw new DatabricksSqlException(
                    $"Unable to get response from Databricks because the SQL statement execution didn't succeed. State: {databricksSqlResponse.State}");
            }

            waitTime *= 2;
            await Task.Delay(waitTime).ConfigureAwait(false);
            Console.WriteLine($"Waiting {waitTime}ms for SQL statement to finish");

            var path = $"{StatementsEndpointPath}/{databricksSqlResponse.StatementId}";
            var httpResponse = await _httpClient.GetAsync(path).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new DatabricksSqlException(
                    $"Unable to get response from Databricks. HTTP status code: {httpResponse.StatusCode}");
            }

            jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            databricksSqlResponse = _responseResponseParser.ParseStatusResponse(jsonResponse);
            LogDatabricksSqlResponseState(databricksSqlResponse);
        }

        if (databricksSqlResponse.State is not SqlResponseState.Succeeded)
        {
            throw new DatabricksSqlException(
                $"Unable to get response from Databricks because the SQL statement execution didn't succeed. State: {databricksSqlResponse.State}");
        }

        return databricksSqlResponse;
    }

    private async Task<SqlChunkResponse> GetChunkAsync(string chunkLink)
    {
        var httpResponse = await _httpClient.GetAsync(chunkLink).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException($"Unable to get chunk from {chunkLink}. HTTP status code: {httpResponse.StatusCode}");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return _responseResponseParser.ParseChunkResponse(jsonResponse);
    }

    private async Task<TableChunk> GetChunkDataAsStringAsync(Uri? externalLink, string[] columnNames)
    {
        var httpResponse = await GetChunkDataAsync(externalLink);
        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return _responseResponseParser.ParseChunkDataResponse(jsonResponse, columnNames);
    }

    private async Task<IAsyncEnumerable<string[]>> GetChunkDataAsStreamAsync(Uri? externalLink)
    {
        var httpResponse = await GetChunkDataAsync(externalLink);

        var jsonResponse = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return _responseResponseParser.ParseChunkDataResponseAsync(jsonResponse);
    }

    private async Task<HttpResponseMessage> GetChunkDataAsync(Uri? externalLink)
    {
        var httpResponse = await _externalHttpClient.GetAsync(externalLink).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException(
                $"Unable to get chunk data from external link {externalLink}. HTTP status code: {httpResponse.StatusCode}");
        }

        return httpResponse;
    }

    private void LogDatabricksRequest(string sqlStatement, List<SqlStatementParameter> sqlStatementParameters)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Executing SQL statement: {Sql}, with parameters: {Parameters}",
                HttpUtility.HtmlEncode(sqlStatement),
                sqlStatementParameters);
        }
    }

    private void LogDatabricksSqlResponseState(SqlResponse response)
    {
        _logger.LogDebug(
            "Databricks SQL response received with state: {State} for statement ID: {StatementId}",
            response.State,
            response.StatementId);
    }
}