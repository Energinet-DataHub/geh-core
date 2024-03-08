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
using System.Runtime.CompilerServices;
using System.Text.Json;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

internal class JsonArrayFormat : IExecuteStrategy
{
    private readonly DatabricksSqlStatementOptions _options;

    public JsonArrayFormat(DatabricksSqlStatementOptions options)
    {
        _options = options;
    }

    public DatabricksStatementRequest GetStatementRequest(DatabricksStatement statement)
        => new(_options.WarehouseId, statement, DatabricksStatementRequest.JsonFormat);

    public async IAsyncEnumerable<dynamic> ExecuteAsync(
        Stream content,
        DatabricksStatementResponse response,
        [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<string[]>(
                           content,
                           cancellationToken: cancellationToken))
        {
            if (record == null) continue;

            IDictionary<string, object?> recordAsObject = new ExpandoObject();
            for (var i = 0; i < record.Length; i++)
            {
                recordAsObject[response.manifest.schema.columns[i].name] = record[i];
            }

            yield return recordAsObject;
        }
    }
}
