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

using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public class DatabricksSqlWarehouseQueryExecutorParallel2 : DatabricksSqlWarehouseQueryExecutor
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _externalHttpClient;
    private readonly DatabricksSqlStatementOptions _options;

    internal DatabricksSqlWarehouseQueryExecutorParallel2(IHttpClientFactory httpClientFactory, IOptions<DatabricksSqlStatementOptions> options)
        : base(httpClientFactory, options)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
        _externalHttpClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
        _options = options.Value;
    }

    public override async IAsyncEnumerable<dynamic> ExecuteStatementAsync(
        DatabricksStatement statement,
        Format format,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var record in ExecuteStatementInternalAsync(statement, format, cancellationToken).ConfigureAwait(false))
        {
            yield return record;
        }
    }

    private async IAsyncEnumerable<dynamic> ExecuteStatementInternalAsync(
        DatabricksStatement statement,
        Format format,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var strategy = format.GetStrategy(_options);
        var request = strategy.GetStatementRequest(statement);
        var response = await request.WaitForSqlWarehouseResultAsync(_httpClient, StatementsEndpointPath, cancellationToken).ConfigureAwait(false);

        if (_httpClient.BaseAddress == null) throw new InvalidOperationException();

        if (response.manifest.total_row_count <= 0 || response.statement_id == null)
        {
            yield break;
        }

        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var tempFolder = CreateRandomTempFolder();
        var downloadTasks = response.manifest.chunks.Select(chunk => DownloadChunkAsync(tempFolder, response.statement_id, chunk, semaphore, cancellationToken)).ToArray();
        Task.WaitAll(downloadTasks, cancellationToken);

        try
        {
            var files = GetFilesOrderByName(tempFolder);
            foreach (var file in files)
            {
                var fs = File.OpenRead(file);
                await using (fs.ConfigureAwait(false))
                {
                    await foreach (var row in strategy.ExecuteAsync(fs, response, cancellationToken).ConfigureAwait(false))
                    {
                        yield return row;
                    }
                }
            }
        }
        finally
        {
            // Cleanup the temporary folder
            Directory.Delete(tempFolder, true);
        }
    }

    private static IEnumerable<string> GetFilesOrderByName(string tempFolder)
    {
        return Directory.GetFiles(tempFolder, "*.file").OrderBy(FileNameSorter);

        static int FileNameSorter(string fileName)
        {
            // Extract the filename without the extension
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Convert the matched numeric part to an integer
            return int.TryParse(fileNameWithoutExtension, out var numericPrefix)
                ? numericPrefix
                : throw new InvalidOperationException("Unable to convert the numeric prefix to an integer.");
        }
    }

    private async Task DownloadChunkAsync(string tempFolder, string statementId, Chunks chunk, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        var uri = StatementsEndpointPath +
                  $"/{statementId}/result/chunks/{chunk.chunk_index}?row_offset={chunk.row_offset}";

        try
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            var chunkResponse = await _httpClient.GetFromJsonAsync<ManifestChunk>(uri, cancellationToken).ConfigureAwait(false);

            if (chunkResponse?.external_links == null) return;

            var filePath = Path.Combine(tempFolder, $"{chunk.chunk_index}.file");
            var stream = await _externalHttpClient.GetStreamAsync(
                chunkResponse.external_links[0].external_link,
                cancellationToken).ConfigureAwait(false);
            var fs = File.OpenWrite(filePath);

            await using (stream.ConfigureAwait(false))
            await using (fs.ConfigureAwait(false))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string CreateRandomTempFolder()
    {
        // Get the path to the user's temporary folder
        var tempPath = Path.GetTempPath();

        // Generate a short, unique folder name
        var folderName = $"{DateTime.Now:yyyyMMddHHmmss}_{new Random().Next(1000, 9999)}";

        // Combine the temporary folder path and the unique folder name
        var fullPath = Path.Combine(tempPath, folderName);

        // Create the new folder
        Directory.CreateDirectory(fullPath);

        return fullPath;
    }
}
