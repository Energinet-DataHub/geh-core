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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Serialization;
using Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests.Assets;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests.Helpers;

public class FakeSqlStatementExecutionClient : ISqlStatementExecutionClient
{
    public async Task<List<TModel>> GetAsync<TModel>(string sqlQuery, Func<List<string>, TModel> mapResult)
    {
        var response = new TestFiles().TimeSeriesResponse;
        var jsonSerializer = new JsonSerializer();
        var jsonResponse = await Task.FromResult(jsonSerializer.Deserialize<StatementExecutionResponseDto>(response));
        return jsonResponse.Result.DataArray.Select(mapResult).ToList();
    }
}
