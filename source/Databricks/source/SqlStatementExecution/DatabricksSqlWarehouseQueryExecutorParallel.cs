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

using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public class DatabricksSqlWarehouseQueryExecutorParallel : DatabricksSqlWarehouseQueryExecutor
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _externalHttpClient;
    private readonly DatabricksSqlStatementOptions _options;

    internal DatabricksSqlWarehouseQueryExecutorParallel(
        IHttpClientFactory httpClientFactory,
        IOptions<DatabricksSqlStatementOptions> options)
        : base(httpClientFactory, options)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
        _externalHttpClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
        _options = options.Value;
    }

    public override async IAsyncEnumerable<dynamic> ExecuteStatementAsync(
        DatabricksStatement statement,
        Format format,
        [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        await foreach (var record in ExecuteStatementInternalAsync(statement, format, cancellationToken).ConfigureAwait(false))
        {
            yield return record;
        }
    }

    /// <summary>
    /// Executes a Databricks SQL statement and returns the results as an asynchronous stream of dynamic objects.
    /// </summary>
    /// <param name="statement">The Databricks SQL statement to execute.</param>
    /// <param name="format">The format in which the results should be returned.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of dynamic objects representing the rows of the result set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the HttpClient BaseAddress is null.</exception>
    /// <remarks>
    /// The method follows these steps:
    /// 1. Retrieves the execution strategy based on the provided format.
    /// 2. Creates a request using the strategy and waits for the SQL warehouse result.
    /// 3. Checks if the total row count in the response is zero or less, and exits if true.
    /// 4. Initializes a bounded channel to manage the processing of chunks in parallel.
    /// 5. Starts a task to process the chunks and write the results to the channel.
    /// 6. Reads from the channel and yields rows in the correct order.
    /// 7. Re-queues out-of-order chunks to ensure rows are yielded in the correct order.
    /// </remarks>
    private async IAsyncEnumerable<dynamic> ExecuteStatementInternalAsync(
        DatabricksStatement statement,
        Format format,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var strategy = format.GetStrategy(_options);
        var request = strategy.GetStatementRequest(statement);
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath, cancellationToken).ConfigureAwait(false);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException("HttpClient BaseAddress is null");

        Debug.WriteLine("Total no of chunks: " + response.manifest.chunks.Length);
        if (response.manifest.total_row_count <= 0)
        {
            yield break;
        }

        var maxBufferedChunks = _options.MaxBufferedChunks;
        Debug.WriteLine("Max buffered chunks: " + maxBufferedChunks);
        var channel = Channel.CreateUnbounded<(long Index, IAsyncEnumerable<dynamic> Rows)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        var processingTask = ProcessChunksAsync(response, strategy, channel.Writer, cancellationToken);

        var nextChunkToProcess = 0;
        var buffer = new SortedDictionary<long, IAsyncEnumerable<dynamic>>();

        await foreach (var (index, rows) in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            buffer[index] = rows;

            while (buffer.TryGetValue(nextChunkToProcess, out var nextRows))
            {
                await foreach (var row in nextRows.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return row;
                }

                buffer.Remove(nextChunkToProcess);
                nextChunkToProcess++;
            }
        }

        await processingTask.ConfigureAwait(false);
    }

    private async Task ProcessChunksAsync(
        DatabricksStatementResponse response,
        IExecuteStrategy strategy,
        ChannelWriter<(long Index, IAsyncEnumerable<dynamic> Rows)> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            var tasks = response.manifest.chunks.Select(chunk =>
                ProcessChunkAsync(response.statement_id, chunk, strategy, response, writer, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        finally
        {
            writer.Complete();
        }
    }

    private async Task ProcessChunkAsync(
        string? statementId,
        Chunks chunk,
        IExecuteStrategy strategy,
        DatabricksStatementResponse response,
        ChannelWriter<(long Index, IAsyncEnumerable<dynamic> Rows)> writer,
        CancellationToken cancellationToken)
    {
        var chunkRowsTask = await FetchAndProcessChunkAsync(statementId, chunk, strategy, response, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync((chunk.chunk_index, chunkRowsTask), cancellationToken).ConfigureAwait(false);
    }

    private async Task<IAsyncEnumerable<dynamic>> FetchAndProcessChunkAsync(
        string? statementId,
        Chunks chunk,
        IExecuteStrategy strategy,
        DatabricksStatementResponse response,
        CancellationToken cancellationToken)
    {
        var uri = $"{StatementsEndpointPath}/{statementId}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";
        var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri, cancellationToken).ConfigureAwait(false);

        if (chunkResponse?.external_links == null) return AsyncEnumerable.Empty<dynamic>();
        var sw = Stopwatch.StartNew();
        var stream = await _externalHttpClient.GetStreamAsync(chunkResponse.external_links[0].external_link, cancellationToken).ConfigureAwait(false);
        sw.Stop();
        Debug.WriteLine($"Fetching chunk {chunk.chunk_index} took {sw.ElapsedMilliseconds} ms");
        return strategy.ExecuteAsync(stream, response, cancellationToken);
    }
}
