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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public sealed class DatabricksSqlWarehouseQueryExecutor : IDatabricksSqlWarehouseQueryExecutor
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly HttpClient _externalHttpClient;
    private readonly DatabricksSqlStatementOptions _options;

    internal DatabricksSqlWarehouseQueryExecutor(
        IHttpClientFactory httpClientFactory,
        IOptions<DatabricksSqlStatementOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
        _externalHttpClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
        _options = options.Value;
    }

    /// <see cref="IDatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(DatabricksStatement)"/>
    public IAsyncEnumerable<dynamic> ExecuteStatementAsync(DatabricksStatement statement)
        => ExecuteStatementAsync(statement, Format.ApacheArrow);

    /// <see cref="IDatabricksSqlWarehouseQueryExecutor.ExecuteStatementAsync(DatabricksStatement, Format)"/>
    public async IAsyncEnumerable<dynamic> ExecuteStatementAsync(DatabricksStatement statement, Format format)
    {
        await foreach (var record in DoExecuteStatementAsync(statement, format))
        {
            yield return record;
        }
    }

    private async IAsyncEnumerable<dynamic> DoExecuteStatementAsync(DatabricksStatement statement, Format format)
    {
        var strategy = format.GetStrategy(_options);
        var request = strategy.GetStatementRequest(statement);
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException();

        if (response.manifest.total_row_count <= 0)
        {
            yield break;
        }

        foreach (var chunk in response.manifest.chunks)
        {
            var uri = StatementsEndpointPath +
                      $"/{response.statement_id}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";
            var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri);

            if (chunkResponse?.external_links == null) continue;

            await using var stream = await _externalHttpClient.GetStreamAsync(chunkResponse.external_links[0].external_link);

            await foreach (var row in strategy.ExecuteAsync(stream, response))
            {
                yield return row;
            }
        }
    }
}
