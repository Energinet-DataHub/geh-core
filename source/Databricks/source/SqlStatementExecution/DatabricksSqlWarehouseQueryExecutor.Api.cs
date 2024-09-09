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

using System.Dynamic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public partial class DatabricksSqlWarehouseQueryExecutor
{
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
        => ExecuteStatementAsync(statement, QueryOptions.Default, cancellationToken);

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
    public virtual IAsyncEnumerable<dynamic> ExecuteStatementAsync(
        DatabricksStatement statement,
        Format format,
        CancellationToken cancellationToken = default)
        => ExecuteStatementAsync(statement, QueryOptions.WithFormat(format), cancellationToken);

    /// <summary>
    /// Asynchronously executes a parameterized SQL query on Databricks and streams the results.
    /// </summary>
    /// <param name="statement">The SQL query to be executed, with a collection of <see cref="QueryParameter"/> parameters.</param>
    /// <param name="options">The options for configuring the query execution, including the format and parallel download settings.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// The cancellation token can be used to implement time out as well.
    /// </param>
    /// <returns>
    /// An asynchronous enumerable of <see cref="ExpandoObject"/> objects representing the result of the query.
    /// </returns>
    /// <remarks>
    /// Use this method to execute SQL queries combined with Parameter Markers against Databricks to protect against SQL injection attacks.
    /// The <paramref name="statement"/> should contain a collection of <see cref="QueryParameter"/>.
    /// Optionally, to simply execute a SQL query without parameters, the collection of <see cref="QueryParameter"/>
    /// can be left empty. However, it is recommended to make use of parameters to protect against SQL injection attacks.
    /// </remarks>
    public virtual async IAsyncEnumerable<dynamic> ExecuteStatementAsync(
        DatabricksStatement statement,
        QueryOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ExecuteStatementInternalAsyncDelegate executeStatementInternalAsync = options.DownloadInParallel
            ? ExecuteStatementParallelInternalAsync
            : ExecuteStatementSerialInternalAsync;

        await foreach (var record in executeStatementInternalAsync(statement, options, cancellationToken).ConfigureAwait(false))
        {
            yield return record;
        }
    }

    private delegate IAsyncEnumerable<dynamic> ExecuteStatementInternalAsyncDelegate(DatabricksStatement statement, QueryOptions options, CancellationToken cancellationToken);

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
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath, cancellationToken).ConfigureAwait(false);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException();

        if (response.manifest.total_row_count <= 0)
        {
            yield break;
        }

        foreach (var chunk in response.manifest.chunks)
        {
            var uri = StatementsEndpointPath +
                      $"/{response.statement_id}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";
            var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri, cancellationToken).ConfigureAwait(false);

            if (chunkResponse?.external_links == null) continue;

            var stream = await _externalHttpClient.GetStreamAsync(
                chunkResponse.external_links[0].external_link,
                cancellationToken).ConfigureAwait(false);
            await using (stream.ConfigureAwait(false))
            {
                await foreach (var row in strategy.ExecuteAsync<T>(stream, cancellationToken).ConfigureAwait(false))
                {
                    yield return row;
                }
            }
        }
    }
}
