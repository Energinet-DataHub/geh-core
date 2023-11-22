﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Apache.Arrow.Ipc;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

internal class StronglyTypedApacheArrowFormat : ApacheArrowFormat
{
    public StronglyTypedApacheArrowFormat(DatabricksSqlStatementOptions options)
        : base(options)
    {
    }

    public async IAsyncEnumerable<T> ExecuteAsync<T>(Stream content)
        where T : class
    {
        using var reader = new ArrowStreamReader(content);

        var batch = await reader.ReadNextRecordBatchAsync();
        while (batch != null)
        {
            for (var i = 0; i < batch.Length; i++)
            {
                yield return batch.ReadRecord<T>(i);
            }

            batch = await reader.ReadNextRecordBatchAsync();
        }
    }
}
