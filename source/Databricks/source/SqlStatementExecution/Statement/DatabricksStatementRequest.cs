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

using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

internal class DatabricksStatementRequest
{
    internal const string JsonFormat = "JSON_ARRAY";
    internal const string CsvFormat = "CSV";
    internal const string ArrowFormat = "ARROW_STREAM";

    internal DatabricksStatementRequest(int timeoutInSeconds, string warehouseId, DatabricksStatement statement, string format)
    {
        Statement = statement.GetSqlStatement();
        Parameters = statement.GetParameters().ToArray();
        WarehouseId = warehouseId;
        Disposition = "EXTERNAL_LINKS";
        WaitTimeout = $"{timeoutInSeconds}s";
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

    [JsonPropertyName("wait_timeout")]
    public string WaitTimeout { get; init; }

    [JsonPropertyName("format")]
    public string Format { get; set; }

    public async ValueTask<DatabricksStatementResponse> WaitForSqlWarehouseResultAsync(HttpClient client, string endpoint)
    {
        DatabricksStatementResponse? response = null;
        var retriesLeft = 10;
        do
        {
            response = await GetResponseFromDataWareHouseAsync(client, endpoint, response);
            if (response.IsSucceeded) return response;
        }
        while ((response.IsPending || response.IsRunning) && retriesLeft-- > 0);

        throw new DatabricksException("Unable to fetch result from Databricks", this, response);
    }

    private async Task<DatabricksStatementResponse> GetResponseFromDataWareHouseAsync(
        HttpClient client,
        string endpoint,
        DatabricksStatementResponse? response)
    {
        if (response == null)
        {
            using var httpResponse = await client.PostAsJsonAsync(endpoint, this);
            response = await httpResponse.Content.ReadFromJsonAsync<DatabricksStatementResponse>();
        }
        else
        {
            await Task.Delay(10);
            var path = $"{endpoint}/{response.statement_id}";
            using var httpResponse = await client.GetAsync(path);
            response = await httpResponse.Content.ReadFromJsonAsync<DatabricksStatementResponse>();
        }

        if (response == null) throw new DatabricksException("Unable to fetch result from Databricks", this);

        return response;
    }
}
