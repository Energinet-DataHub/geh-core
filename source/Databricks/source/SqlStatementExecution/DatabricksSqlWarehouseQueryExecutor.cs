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
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public class DatabricksSqlWarehouseQueryExecutor
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabricksSqlWarehouseQueryExecutor"/>
    /// class for mocking.
    /// </summary>
    protected DatabricksSqlWarehouseQueryExecutor()
    {
        _httpClient = null!;
        _externalHttpClient = null!;
        _options = null!;
    }

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results using <see cref="Format.ApacheArrow"/> format.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other object or threads to receive notice of cancellation.
    /// The cancellation token can be used to implement time out as well.</param>
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
    public virtual IAsyncEnumerable<dynamic> ExecuteStatementAsync(
        DatabricksStatement statement,
        CancellationToken cancellationToken = default)
        => ExecuteStatementAsync(statement, Format.ApacheArrow, cancellationToken);

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <param name="format">The desired format of the data returned.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other object or threads to receive notice of cancellation.
    /// The cancellation token can be used to implement time out as well.</param>
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
    public virtual async IAsyncEnumerable<dynamic> ExecuteStatementAsync(
        DatabricksStatement statement,
        Format format,
        [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        await foreach (var record in DoExecuteStatementAsync(statement, format, cancellationToken))
        {
            yield return record;
        }
    }

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the result back as a collection of strongly typed objects.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with collection of <see cref="QueryParameter"/> parameters.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other object or threads to receive notice of cancellation.
    /// The cancellation token can be used to implement time out as well.</param>
    /// <typeparam name="T">Target type</typeparam>
    /// <returns>An asynchronous enumerable of <typeparamref name="T"/> representing the result of the query.</returns>
    /// <remarks>
    /// This is an experimental feature and may be removed in a future version.
    /// <br/><br/>
    /// Requirements for <typeparamref name="T"/>:<br/>
    /// - Must be a reference type<br/>
    /// - Must have a public constructor with parameters matching the columns in the result set<br/>
    /// - Must only have a single constructor<br/>
    /// - Must be annotated with <see cref="ArrowFieldAttribute"/> to indicate the order of the constructor parameters
    /// </remarks>
    public virtual async IAsyncEnumerable<T> ExecuteStatementAsync<T>(
        DatabricksStatement statement,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class
    {
        var strategy = new StronglyTypedApacheArrowFormat(_options);
        var request = strategy.GetStatementRequest(statement);
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath, cancellationToken);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException();

        if (response.manifest.total_row_count <= 0)
        {
            yield break;
        }

        foreach (var chunk in response.manifest.chunks)
        {
            var uri = StatementsEndpointPath +
                      $"/{response.statement_id}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";
            var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri, cancellationToken);

            if (chunkResponse?.external_links == null) continue;

            await using var stream = await _externalHttpClient.GetStreamAsync(
                chunkResponse.external_links[0].external_link,
                cancellationToken);

            await foreach (var row in strategy.ExecuteAsync<T>(stream, cancellationToken))
            {
                yield return row;
            }
        }
    }

    private async IAsyncEnumerable<dynamic> DoExecuteStatementAsync(
        DatabricksStatement statement,
        Format format,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        var strategy = format.GetStrategy(_options);
        var request = strategy.GetStatementRequest(statement);
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath, cancellationToken);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException();

        if (response.manifest.total_row_count <= 0)
        {
            yield break;
        }

        foreach (var chunk in response.manifest.chunks)
        {
            var uri = StatementsEndpointPath +
                      $"/{response.statement_id}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";
            var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri, cancellationToken);

            if (chunkResponse?.external_links == null) continue;

            await using var stream = await _externalHttpClient.GetStreamAsync(chunkResponse.external_links[0].external_link, cancellationToken);

            await foreach (var row in strategy.ExecuteAsync(stream, response, cancellationToken))
            {
                yield return row;
            }
        }
    }
}
