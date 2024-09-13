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
using System.Text.Json.Serialization;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

/// <summary>
/// Some properties are being serialized and passed to the Databricks SQL Statement Execution API.
/// Learn more about the API here: https://docs.databricks.com/api/azure/workspace/statementexecution.
/// </summary>
internal class DatabricksStatementRequest
{
    internal const string JsonFormat = "JSON_ARRAY";
    internal const string ArrowFormat = "ARROW_STREAM";
    private const int MaxWaitTimeForLoopInMilliseconds = 10000;

    internal DatabricksStatementRequest(string warehouseId, DatabricksStatement statement, string format)
    {
        Statement = statement.GetSqlStatement();
        Parameters = statement.GetParameters().ToArray();
        WarehouseId = warehouseId;
        Disposition = "EXTERNAL_LINKS";
        WaitTimeout = "0s";
        Format = format;
    }

    [JsonPropertyName("parameters")]
    public QueryParameter[] Parameters { get; init; }

    [JsonPropertyName("statement")]
    public string Statement { get; init; }

    [JsonPropertyName("warehouse_id")]
    public string WarehouseId { get; init; }

    [JsonPropertyName("disposition")]
    public string Disposition { get; init; }

    /// <summary>
    /// Solely used to set the timeout for the first Databricks API invocation.
    /// If the result wasn't included then the <see cref="DatabricksStatementRequest"/> will repeatedly
    /// check for the result until it's available.
    /// </summary>
    [JsonPropertyName("wait_timeout")]
    public string WaitTimeout { get; init; }

    [JsonPropertyName("format")]
    public string Format { get; init; }

    public async ValueTask<DatabricksStatementResponse> WaitForSqlWarehouseResultAsync(HttpClient client, string endpoint, CancellationToken cancellationToken)
    {
        DatabricksStatementResponse? response = null;
        var fibonacci = new Fibonacci();
        do
        {
            try
            {
                var delayInMilliseconds = Math.Min(fibonacci.GetNextNumber() * 10, MaxWaitTimeForLoopInMilliseconds);
                response = await GetResponseFromDataWarehouseAsync(client, endpoint, response, delayInMilliseconds, cancellationToken).ConfigureAwait(false);
                if (response.IsSucceeded)
                    return response;

                if (cancellationToken.IsCancellationRequested)
                {
                    if (string.IsNullOrEmpty(response.statement_id))
                        throw new InvalidOperationException("The statement_id is missing from databricks response");

                    // Cancel the statement without cancellation token since it is already cancelled
                    await CancelStatementAsync(client, endpoint, response.statement_id).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (TaskCanceledException tce)
            {
                if (!string.IsNullOrEmpty(response?.statement_id))
                    await CancelStatementAsync(client, endpoint, response.statement_id).ConfigureAwait(false);
                throw new OperationCanceledException("The operation was cancelled", tce, cancellationToken);
            }
        }
        while (response.IsPending || response.IsRunning);

        throw new DatabricksException(this, response);
    }

    private async Task<DatabricksStatementResponse> GetResponseFromDataWarehouseAsync(
        HttpClient client,
        string endpoint,
        DatabricksStatementResponse? response,
        int delayInMilliseconds,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(delayInMilliseconds, nameof(delayInMilliseconds));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delayInMilliseconds, MaxWaitTimeForLoopInMilliseconds, nameof(delayInMilliseconds));

        if (response == null)
        {
            // No cancellation token is used because we want to wait for the result
            // With the response we are able to cancel the statement if needed
            // ReSharper disable MethodSupportsCancellation
#pragma warning disable CA2016
            using var httpResponse = await client.PostAsJsonAsync(endpoint, this).ConfigureAwait(false);
            response = await httpResponse.Content.ReadFromJsonAsync<DatabricksStatementResponse>().ConfigureAwait(false);
#pragma warning restore CA2016
            // ReSharper restore MethodSupportsCancellation
        }
        else
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds), cancellationToken).ConfigureAwait(false);

            var path = $"{endpoint}/{response.statement_id}";
            using var httpResponse = await client.GetAsync(path, cancellationToken).ConfigureAwait(false);
            response = await httpResponse.Content.ReadFromJsonAsync<DatabricksStatementResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return response ?? throw new DatabricksException(this);
    }

    private static async Task CancelStatementAsync(HttpClient client, string endpoint, string statementId)
    {
        var path = $"{endpoint}/{statementId}/cancel";
        using var httpResponse = await client.PostAsync(path, new StringContent(string.Empty)).ConfigureAwait(false);
    }
}
