// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Json;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Constants;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Client;

public sealed class DatabricksSqlWarehouseQueryExecutor
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private readonly HttpClient _externalHttpClient;
    private readonly DatabricksSqlStatementOptions _options;

    public DatabricksSqlWarehouseQueryExecutor(
        IHttpClientFactory httpClientFactory,
        IOptions<DatabricksSqlStatementOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
        _externalHttpClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
        _options = options.Value;
    }

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results using <see cref="Format.ApacheArrow"/> format.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <returns>
    /// An asynchronous enumerable of <typeparamref name="T"/> object representing the result of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="statement"/> should contain a collection of <see cref="QueryParameter"/>.
    ///
    /// Optionally, to simply execute a SQL query without parameters, the collection of <see cref="QueryParameter"/>
    /// can be left empty. However, it is recommended to make use of parameters to protect against SQL injection attacks.
    /// </remarks>
    public IAsyncEnumerable<T> ExecuteStatementAsync<T>(DatabricksStatement statement)
        => ExecuteStatementAsync<T>(statement, Format.ApacheArrow);

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <param name="format">The desired format of the data returned.</param>
    /// <returns>
    /// An asynchronous enumerable of <typeparamref name="T"/> object representing the result of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="statement"/> should contain a collection of <see cref="QueryParameter"/>.
    ///
    /// Optionally, to simply execute a SQL query without parameters, the collection of <see cref="QueryParameter"/>
    /// can be left empty. However, it is recommended to make use of parameters to protect against SQL injection attacks.
    /// </remarks>
    public async IAsyncEnumerable<T> ExecuteStatementAsync<T>(DatabricksStatement statement, Format format)
    {
        await foreach (var record in DoExecuteStatementAsync(statement, format))
        {
            yield return statement.Create(record);
        }
    }

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results using <see cref="Format.ApacheArrow"/> format.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="ExpandoObject"/> object representing the result of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="statement"/> should contain a collection of <see cref="QueryParameter"/>.
    ///
    /// Optionally, to simply execute a SQL query without parameters, the collection of <see cref="QueryParameter"/>
    /// can be left empty. However, it is recommended to make use of parameters to protect against SQL injection attacks.
    /// </remarks>
    public IAsyncEnumerable<dynamic> ExecuteStatementAsync(DatabricksStatement statement)
        => ExecuteStatementAsync(statement, Format.ApacheArrow);

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <param name="format">The desired format of the data returned.</param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="ExpandoObject"/> object representing the result of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="statement"/> should contain a collection of <see cref="QueryParameter"/>.
    ///
    /// Optionally, to simply execute a SQL query without parameters, the collection of <see cref="QueryParameter"/>
    /// can be left empty. However, it is recommended to make use of parameters to protect against SQL injection attacks.
    /// </remarks>
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
        var sw = Stopwatch.StartNew();
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath);
        // Metrics.RecordWarehouseDuration(sw.Elapsed);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException();

        if (response.manifest.total_row_count <= 0)
        {
            yield break;
        }

        foreach (var chunk in response.manifest.chunks)
        {
            sw.Restart();
            var uri = StatementsEndpointPath +
                      $"/{response.statement_id}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";
            var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri);
            // Metrics.RecordChunkDuration(sw.Elapsed);

            if (chunkResponse?.external_links == null) continue;

            sw.Restart();
            await using var stream = await _externalHttpClient.GetStreamAsync(chunkResponse.external_links[0].external_link);
            // Metrics.RecordDurationOfDataRetrieval(sw.Elapsed);

            await foreach (var row in strategy.ExecuteAsync(stream, response))
            {
                yield return row;
            }
        }
    }
}
