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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Databricks;

/// <summary>
/// A manager for managing Databricks SQL schemas and tables from integration tests.
/// </summary>
public class DatabricksSchemaManager
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;
    private bool _schemaExists = false;

    public DatabricksSchemaManager(DatabricksSettings databricksSettings, string schemaPrefix)
    {
        DatabricksSettings = databricksSettings ?? throw new ArgumentNullException(nameof(databricksSettings));

        _httpClient = CreateHttpClient(DatabricksSettings);
        SchemaName = $"{schemaPrefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}";
    }

    public string SchemaName { get; }

    public bool SchemaExists => _schemaExists;

    private DatabricksSettings DatabricksSettings { get; }

    /// <summary>
    /// Create schema (formerly known as database).
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateSchemaAsync()
    {
        var sqlStatement = $"CREATE SCHEMA IF NOT EXISTS {SchemaName}";
        await ExecuteSqlAsync(sqlStatement);
        _schemaExists = true;
    }

    /// <summary>
    /// Dropping schema (formerly known as database) with all tables, views etc.
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task DropSchemaAsync()
    {
        var sqlStatement = $"DROP SCHEMA {SchemaName} CASCADE";
        await ExecuteSqlAsync(sqlStatement);
        _schemaExists = false;
    }

    /// <summary>
    /// Create table with a given name and specified column definition (column name, (data type, is nullable))
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task<string> CreateTableAsync(string tableName, Dictionary<string, (string DataType, bool IsNullable)> columnDefinition)
    {
        var columnDefinitions =
            string.Join(", ", columnDefinition.Select(c =>
                $"{c.Key} {c.Value.DataType}{(c.Value.IsNullable ? string.Empty : " NOT NULL")}"));

        var sqlStatement = $"CREATE TABLE IF NOT EXISTS {SchemaName}.{tableName} ({columnDefinitions})";
        await ExecuteSqlAsync(sqlStatement);
        return tableName;
    }

    /// <summary>
    /// Create a unique table name.
    /// </summary>
    public static string CreateTableName(string prefix = "TestTable")
    {
        var tableName = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
        return tableName;
    }

    /// <summary>
    /// Inserts rows into a table for all columns. The rows are specified as a list of lists of strings. Example:
    /// INSERT INTO myschema.mytable VALUES (10, 'John', 'Doe', 30, 'john.doe@domain', 1.234), (20, 'Jane', 'Doe', 28, 'jane.doe@domain', 4.321);
    /// INSERT INTO myschema.mytable VALUES ('someString', 'someOtherString', 1.234), ('anotherString', 'anotherOtherString', 2.345);
    /// </summary>
    /// <param name="tableName">Name of table</param>
    /// <param name="rows">Rows to be inserted in table. Note: that strings should have single quotes around them.
    /// </param>
    public async Task InserAsync(string tableName, IEnumerable<IEnumerable<string>> rows)
    {
        var values = string.Join(", ", rows.Select(row => $"({string.Join(", ", row.Select(val => $"{val}"))})"));
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} VALUES {values}";
        await ExecuteSqlAsync(sqlStatement);
    }

    /// <summary>
    /// Inserts rows into a table for selected columns. The rows are specified as a list of lists of strings. Example:
    /// INSERT INTO myschema.mytable ('id', 'firstname', 'age' ) VALUES (10, 'John', 30), (20, 'Jane', 28);
    /// </summary>
    /// <param name="tableName">Name of table</param>
    /// <param name="columnNames">Names of the columns that rows are referring to</param>
    /// <param name="rows">Rows to be inserted in table. Note: that strings should have single quotes around them.
    /// </param>
    public async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<IEnumerable<string>> rows)
    {
        var columnsNames = string.Join(", ", columnNames);
        var values = string.Join(", ", rows.Select(row => $"({string.Join(", ", row.Select(val => $"{val}"))})"));
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} ({columnsNames}) VALUES {values}";
        await ExecuteSqlAsync(sqlStatement);
    }

    /// <summary>
    /// Inserts a single row into a table for selected columns. Example:
    /// INSERT INTO myschema.mytable ('id', 'firstname', 'age' ) VALUES (10, 'John', 30);
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnNames"></param>
    /// <param name="row"></param>
    public async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<string> row)
    {
        var columnsNames = string.Join(", ", columnNames);
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} ({columnsNames}) VALUES ({string.Join(",", row)})";
        await ExecuteSqlAsync(sqlStatement);
    }

    /// <summary>
    /// Inserts a single row into a table, for all columns. Example:
    /// INSERT INTO myschema.mytable VALUES (10, 'John', 'Doe', 30, 'john.doe@domain');
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="row"></param>
    public async Task InsertAsync(string tableName, IEnumerable<string> row)
    {
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} VALUES ({string.Join(",", row)})";
        await ExecuteSqlAsync(sqlStatement);
    }

    protected virtual async Task ExecuteSqlAsync(string sqlStatement)
    {
        sqlStatement = sqlStatement.Trim();

        if (string.IsNullOrEmpty(sqlStatement))
        {
            return;
        }

        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = DatabricksSettings.WarehouseId,
        };
        var jsonRequest = JsonConvert.SerializeObject(requestObject);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
        var httpResponse = await _httpClient.PostAsync(StatementsEndpointPath, content).ConfigureAwait(false);

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Unable to execute SQL statement on Databricks. Status code: {httpResponse.StatusCode}");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var state = jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
        if (state != "SUCCEEDED")
        {
            throw new Exception($"Failed to execute SQL statement: {sqlStatement}. Response: {jsonResponse}");
        }
    }

    protected virtual HttpClient CreateHttpClient(DatabricksSettings databricksOptions)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(databricksOptions.WorkspaceUrl),
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", databricksOptions.WorkspaceAccessToken);

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

        return httpClient;
    }
}
