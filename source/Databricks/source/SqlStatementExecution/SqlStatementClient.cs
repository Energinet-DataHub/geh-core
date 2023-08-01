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
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0
// https://learn.microsoft.com/en-gb/azure/databricks/sql/api/sql-execution-tutorial
public class SqlStatementClient : ISqlStatementClient
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly DatabricksOptions _databricksDatabricksOptions;
    private readonly IDatabricksSqlResponseParser _responseResponseParser;

    public SqlStatementClient(
        HttpClient httpClient,
        DatabricksOptions databricksOptions,
        IDatabricksSqlResponseParser responseResponseParser)
    {
        _httpClient = httpClient;
        _databricksDatabricksOptions = databricksOptions;
        _responseResponseParser = responseResponseParser;
        ConfigureHttpClient(_httpClient, _databricksDatabricksOptions);
    }

    public async IAsyncEnumerable<SqlResultRow> ExecuteAsync(string sqlStatement)
    {
        var response = await GetFirstChunkOrNullAsync(sqlStatement).ConfigureAwait(false);
        var columnNames = response.ColumnNames;
        var chunk = response.Chunk;

        while (chunk != null)
        {
            var data = await GetChunkDataAsync(chunk.ExternalLink, columnNames!).ConfigureAwait(false);

            for (var index = 0; index < data.Rows.Count; index++)
            {
                yield return new SqlResultRow(data, index);
            }

            if (chunk.NextChunkInternalLink == null)
            {
                break;
            }

            chunk = await GetChunkAsync(chunk.NextChunkInternalLink).ConfigureAwait(false);
        }
    }

    private async Task<DatabricksSqlResponse> GetFirstChunkOrNullAsync(string sqlStatement)
    {
        const int timeOutPerAttemptSeconds = 30;

        var requestObject = new
        {
            wait_timeout = $"{timeOutPerAttemptSeconds}s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = _databricksDatabricksOptions.WarehouseId,
            disposition = "EXTERNAL_LINKS", // Some results are larger than the maximum allowed 16MB limit, thus we need to use external links
        };
        var response = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException($"Unable to get calculation result from Databricks. HTTP status code: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var databricksSqlResponse = _responseResponseParser.ParseStatusResponse(jsonResponse);

        while (databricksSqlResponse.State is DatabricksSqlResponseState.Pending or DatabricksSqlResponseState.Running)
        {
            var path = $"{StatementsEndpointPath}/{databricksSqlResponse.StatementId}";
            var httpResponse = await _httpClient.GetAsync(path).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new DatabricksSqlException($"Unable to get calculation result from Databricks. HTTP status code: {httpResponse.StatusCode}");
            }

            databricksSqlResponse = _responseResponseParser.ParseStatusResponse(jsonResponse);
        }

        if (databricksSqlResponse.State is DatabricksSqlResponseState.Cancelled or DatabricksSqlResponseState.Failed or DatabricksSqlResponseState.Closed)
        {
            throw new DatabricksSqlException($"Unable to get calculation result from Databricks because the SQL statement execution didn't succeed. State: {databricksSqlResponse.State}");
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

    private async Task<TableChunk> GetChunkDataAsync(Uri externalLink, string[] columnNames)
    {
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(externalLink).ConfigureAwait(false);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new DatabricksSqlException($"Unable to get chunk data from external link {externalLink}. HTTP status code: {httpResponse.StatusCode}");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return _responseResponseParser.ParseChunkDataResponse(jsonResponse, columnNames);
    }

    private static void ConfigureHttpClient(HttpClient httpClient, DatabricksOptions options)
    {
        httpClient.BaseAddress = new Uri(options.WorkspaceUrl);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.WorkspaceToken);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        httpClient.BaseAddress = new Uri(options.WorkspaceUrl);
    }
}
