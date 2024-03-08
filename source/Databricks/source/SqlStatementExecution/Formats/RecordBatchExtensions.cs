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

using Apache.Arrow;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;

internal static class RecordBatchExtensions
{
    public static T ReadRecord<T>(this RecordBatch batch, int row)
        where T : class
    {
        var fieldNames = Reflections.GetArrowFieldNames<T>();
        var values = fieldNames.Select(field => batch.Column(field).GetValue(row)).ToArray();
        return Reflections.CreateInstance<T>(values);
    }
}
