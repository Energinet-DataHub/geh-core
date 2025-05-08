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

using System.Text;
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
    private bool _schemaExists;

    public DatabricksSchemaManager(IHttpClientFactory factory, DatabricksSettings databricksSettings, string schemaPrefix)
    {
        DatabricksSettings = databricksSettings ?? throw new ArgumentNullException(nameof(databricksSettings));

        _httpClient = factory.CreateHttpClient(DatabricksSettings);
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
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
        _schemaExists = true;
    }

    /// <summary>
    /// Dropping schema (formerly known as database) with all tables, views etc.
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task DropSchemaAsync()
    {
        var sqlStatement = $"DROP SCHEMA {SchemaName} CASCADE";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
        _schemaExists = false;
    }

    /// <summary>
    /// Create table with a given name and specified column definition (column name, (data type, is nullable))
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateTableAsync(string tableName, Dictionary<string, (string DataType, bool IsNullable)> columnDefinition)
    {
        var columnDefinitions =
            string.Join(", ", columnDefinition.Select(c =>
                $"{c.Key} {c.Value.DataType}{(c.Value.IsNullable ? string.Empty : " NOT NULL")}"));

        var sqlStatement = $"CREATE TABLE IF NOT EXISTS {SchemaName}.{tableName} ({columnDefinitions})";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
    }

    /// <summary>
    /// Drops a table with a given name.
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task DropTableAsync(string tableName)
    {
        var sqlStatement = $"DROP TABLE IF EXISTS {SchemaName}.{tableName}";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a unique table name.
    /// </summary>
    public static string CreateTableName(string prefix = "TestTable")
    {
        var tableName = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
        return tableName;
    }

    /// <summary>
    /// Inserts rows into a table for all columns. The rows are specified as a list of lists of strings.
    /// </summary>
    /// <example>INSERT INTO SchemaName.<paramref name="tableName"/> VALUES (10, 'John', 30), (20, 'Jane', 28)</example>
    /// <param name="tableName">Name of table</param>
    /// <param name="rows">Rows to be inserted in table. Note: that strings should have single quotes around them.</param>
    public async Task InsertAsync(string tableName, IEnumerable<IEnumerable<string>> rows)
    {
        var values = string.Join(", ", rows.Select(row => $"({string.Join(", ", row.Select(val => $"{val}"))})"));
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} VALUES {values}";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts rows into a table for selected columns. The rows are specified as a list of lists of strings.
    /// </summary>
    /// <example>INSERT INTO SchemaName.<paramref name="tableName"/> ('id', 'firstname', 'age' ) VALUES (10, 'John', 30), (20, 'Jane', 28)</example>
    /// <param name="tableName">Name of table</param>
    /// <param name="columnNames">Names of the columns that rows are referring to</param>
    /// <param name="rows">Rows to be inserted in table. Note: that strings should have single quotes around them.</param>
    public async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<IEnumerable<string>> rows)
    {
        var columnsNames = string.Join(", ", columnNames);
        var values = string.Join(", ", rows.Select(row => $"({string.Join(", ", row.Select(val => $"{val}"))})"));
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} ({columnsNames}) VALUES {values}";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a single row into a table for selected columns.
    /// </summary>
    /// <example>INSERT INTO SchemaName.<paramref name="tableName"/> ('id', 'firstname', 'age' ) VALUES (10, 'John', 30)</example>
    /// <param name="tableName"></param>
    /// <param name="columnNames"></param>
    /// <param name="row"></param>
    public async Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<string> row)
    {
        var columnsNames = string.Join(", ", columnNames);
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} ({columnsNames}) VALUES ({string.Join(", ", row)})";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts a single row into a table, for all columns.
    /// </summary>
    /// <example>INSERT INTO SchemaName.<paramref name="tableName"/> VALUES (10, 'John', 'Doe', 30, 'john.doe@domain');</example>

    /// <param name="tableName"></param>
    /// <param name="row"></param>
    public async Task InsertAsync(string tableName, IEnumerable<string> row)
    {
        var sqlStatement = $"INSERT INTO {SchemaName}.{tableName} VALUES ({string.Join(", ", row)})";
        await ExecuteSqlAsync(sqlStatement).ConfigureAwait(false);
    }

    private async Task ExecuteSqlAsync(string sqlStatement)
    {
        sqlStatement = sqlStatement.Trim();
        if (string.IsNullOrEmpty(sqlStatement))
            return;

        var jsonRequest = PrepareJsonRequest(sqlStatement);
        var httpResponse = await PostSqlAsync(jsonRequest).ConfigureAwait(false);

        EnsureSuccessfulResponse(httpResponse);
        await ValidateResponseAsync(httpResponse.Content).ConfigureAwait(false);
    }

    private string PrepareJsonRequest(string sqlStatement)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // setting a timeout other than 0s makes the operation synchronous
            statement = sqlStatement,
            warehouse_id = DatabricksSettings.WarehouseId,
        };

        return JsonConvert.SerializeObject(requestObject);
    }

    private async Task<HttpResponseMessage> PostSqlAsync(string jsonRequest)
    {
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(StatementsEndpointPath, content).ConfigureAwait(false);
    }

    private void EnsureSuccessfulResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unable to execute SQL statement on Databricks. Status code: {response.StatusCode}");
        }
    }

    private async Task ValidateResponseAsync(HttpContent content)
    {
        var jsonResponse = await content.ReadAsStringAsync().ConfigureAwait(false);
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var state = jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
        if (state != "SUCCEEDED")
        {
            throw new Exception($"Failed to execute SQL statement. Response: {jsonResponse}");
        }
    }
}
