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

using System.Globalization;
using CsvHelper;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Databricks;

public static class DatabricksSchemaManagerExtensions
{
    public const string DeltaTableArrayOfStrings = "ARRAY<STRING>";

    /// <summary>
    /// Expects a CSV file which was exported from Databricks.
    /// The header must contain the column names and each row must contain the delta table values per column.
    /// Delta table arrays must be parsed differently, but otherwise all values can be parsed from the CSV into strings.
    /// All parsed rows are then inserted into a delta table.
    /// </summary>
    public static async Task InsertFromCsvFileAsync(
        this DatabricksSchemaManager schemaManager,
        string tableName,
        Dictionary<string, (string DataType, bool IsNullable)> columnDefinitions,
        string csvFilePath)
    {
        using (var streamReader = new StreamReader(csvFilePath))
        using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
        {
            await csvReader.ReadAsync().ConfigureAwait(false);
            csvReader.ReadHeader();

            if (csvReader.HeaderRecord == null)
                throw new InvalidOperationException("First row of CSV file must contain header information.");

            var rows = new List<string[]>();
            while (await csvReader.ReadAsync().ConfigureAwait(false))
            {
                var row = new string[csvReader.HeaderRecord.Length];
                for (var columnIndex = 0; columnIndex < csvReader.ColumnCount; columnIndex++)
                {
                    row[columnIndex] = ParseColumnValue(columnDefinitions, csvReader, columnIndex);
                }

                rows.Add(row);
            }

            await schemaManager.InsertAsync(tableName, csvReader.HeaderRecord, rows).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Parse CSV column value into a delta table "insertable" value.
    /// Only arrays require special handling; all other values can be inserted as "strings".
    /// </summary>
    private static string ParseColumnValue(
        Dictionary<string, (string DataType, bool IsNullable)> columnDefinitions,
        CsvReader csvReader,
        int columnIndex)
    {
        var columnName = csvReader.HeaderRecord![columnIndex];
        var columnValue = csvReader.GetField(columnIndex);

        if (columnDefinitions[columnName].IsNullable && columnValue == string.Empty)
        {
            return "NULL";
        }

        if (columnDefinitions[columnName].DataType == DeltaTableArrayOfStrings)
        {
            var arrayContent = columnValue!
                .Replace('[', '(')
                .Replace(']', ')');

            return $"Array{arrayContent}";
        }

        return $"'{columnValue}'";
    }
}
