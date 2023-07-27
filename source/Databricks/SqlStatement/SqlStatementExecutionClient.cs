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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Serialization;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution
{
    internal class SqlStatementExecutionClient : ISqlStatementExecutionClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly DatabricksOptions _databricksOptions;

        public SqlStatementExecutionClient(
            IHttpClientFactory httpClientFactory,
            IJsonSerializer jsonSerializer,
            DatabricksOptions databricksOptions)
        {
            _httpClientFactory = httpClientFactory;
            _jsonSerializer = jsonSerializer;
            _databricksOptions = databricksOptions;
        }

        public async Task<List<TModel>> GetAsync<TModel>(string sqlQuery, Func<List<string>, TModel> mapResult)
        {
            var client = _httpClientFactory.CreateClient("DatabricksStatementExecutionApi");
            var request = CreateRequest(sqlQuery);
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = await DeserializeResponseContentAsync(response).ConfigureAwait(false);

            if (responseContent.Status.State != "SUCCEEDED")
            {
                throw new Exception($"Unable to get result from Databricks. State: {responseContent.Status.State}");
            }

            var mappedResult = responseContent.Result.DataArray.Select(mapResult).ToList();

            return mappedResult;
        }

        private async Task<StatementExecutionResponseDto> DeserializeResponseContentAsync(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return _jsonSerializer.Deserialize<StatementExecutionResponseDto>(jsonString);
        }

        private HttpRequestMessage CreateRequest(string sqlQuery)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/2.0/sql/statements", UriKind.Relative));
            request.Content = JsonContent.Create(new
            {
                warehouse_id = _databricksOptions.WarehouseId,
                catalog = "hive_metastore",
                schema = _databricksOptions.TableName,
                statement = sqlQuery,
            });

            return request;
        }
    }
}
