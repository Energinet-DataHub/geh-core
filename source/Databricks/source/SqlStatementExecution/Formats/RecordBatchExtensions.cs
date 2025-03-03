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
    public static T ReadRecord<T>(this RecordBatch batch, int row, ReflectionStrategy reflectionStrategy)
        where T : class
    {
        return DebugInfo.Measure(
            "RecordBatchExtensions.ReadRecord",
            () => CreateObject<T>(batch, row, reflectionStrategy));
    }

    private static T CreateObject<T>(RecordBatch batch, int row, ReflectionStrategy reflectionStrategy)
        where T : class
    {
        return reflectionStrategy switch
        {
            ReflectionStrategy.Default => Standard<T>(batch, row),
            ReflectionStrategy.Cache => Cache<T>(batch, row),
            ReflectionStrategy.Lambda => Lambda<T>(batch, row),
            _ => throw new ArgumentOutOfRangeException(nameof(reflectionStrategy), reflectionStrategy, null),
        };
    }

    private static T Standard<T>(RecordBatch batch, int row)
    {
        var fieldNames = Reflections.GetArrowFieldNames<T>();
        var values = fieldNames.Select(field => batch.Column(field).GetValue(row)).ToArray();
        return Reflections.CreateInstance<T>(values);
    }

    private static T Cache<T>(RecordBatch batch, int row)
    {
        var fieldNames = ReflectionsCache.GetArrowFieldNames<T>();
        var values = fieldNames.Select(field => batch.Column(field).GetValue(row)).ToArray();
        return ReflectionsCache.CreateInstance<T>(values);
    }

    private static T Lambda<T>(RecordBatch batch, int row)
    {
        var fieldNames = ReflectionsLambda.GetArrowFieldNames<T>();
        var values = fieldNames.Select(field => batch.Column(field).GetValue(row)).ToArray();
        return ReflectionsLambda.CreateInstance<T>(values);
    }
}
