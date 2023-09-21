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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.AppSettings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SqlStatementExecution.IntegrationTests.Fixtures;

/// <summary>
/// A manager for managing Databricks SQL schemas and tables from integration tests.
/// </summary>
public class DatabricksSchemaManager
{
    private const string StatementsEndpointPath = "/api/2.0/sql/statements";
    private readonly HttpClient _httpClient;

    public DatabricksSchemaManager(DatabricksOptions databricksOptions, string schemaPrefix)
    {
        DatabricksOptions = databricksOptions ?? throw new ArgumentNullException(nameof(databricksOptions));

        _httpClient = HttpClientFactory.CreateHttpClient(DatabricksOptions);
        SchemaName = $"{schemaPrefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString()[..8]}";
    }

    public string SchemaName { get; }

    // TODO JMG: Consider if we can hide these settings or ensure they are readonly in DatabricksWarehouseSettings,
    // otherwise external developers can manipulate them even after we created the manager
    private DatabricksOptions DatabricksOptions { get; }

    /// <summary>
    /// Create schema (formerly known as database).
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task CreateSchemaAsync()
    {
        var sqlStatement = @$"CREATE SCHEMA {SchemaName}";
        await ExecuteSqlAsync(sqlStatement);
    }

    /// <summary>
    /// Create table with a specified column definition (column name, data type)
    /// See more here https://docs.databricks.com/lakehouse/data-objects.html.
    /// </summary>
    public async Task<string> CreateTableAsync(Dictionary<string, string> columnDefinition)
    {
        var tableName = $"TestTable_{DateTime.Now:yyyyMMddHHmmss}";
        var columnDefinitions = string.Join(", ", columnDefinition.Select(c => $"{c.Key} {c.Value}"));
        var sqlStatement = $@"CREATE TABLE {SchemaName}.{tableName} ({columnDefinitions})";
        await ExecuteSqlAsync(sqlStatement);
        return tableName;
    }

    /// <summary>
    /// Inserts rows into a table. The rows are specified as a list of lists of strings. Example:
    /// INSERT INTO myschema.mytable VALUES ('someString', 'someOtherString', 1.234), ('anotherString', 'anotherOtherString', 2.345);
    /// </summary>
    /// <param name="tableName">Name of table</param>
    /// <param name="rows">Rows to be inserted in table. Note: that strings should have single quotes around them.
    /// </param>
    public async Task InsertIntoAsync(string tableName, IEnumerable<IEnumerable<string>> rows)
    {
        var values = string.Join(", ", rows.Select(row => $"({string.Join(", ", row.Select(val => $"{val}"))})"));
        var sqlStatement = $@"INSERT INTO {SchemaName}.{tableName} VALUES {values}";
        await ExecuteSqlAsync(sqlStatement);
    }

    public async Task InsertIntoAsync(string tableName, IEnumerable<string> row)
    {
        var sqlStatement = $@"INSERT INTO {SchemaName}.{tableName} VALUES ({string.Join(",", row)})";
        await ExecuteSqlAsync(sqlStatement);
    }

    public async Task DropSchemaAsync()
    {
        var sqlStatement = @$"DROP SCHEMA {SchemaName} CASCADE";
        await ExecuteSqlAsync(sqlStatement);
    }

    private async Task ExecuteSqlAsync(string sqlStatement)
    {
        var requestObject = new
        {
            on_wait_timeout = "CANCEL",
            wait_timeout = $"50s", // Make the operation synchronous
            statement = sqlStatement,
            warehouse_id = DatabricksOptions.WarehouseId,
        };
        var httpResponse = await _httpClient.PostAsJsonAsync(StatementsEndpointPath, requestObject).ConfigureAwait(false);

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new SqlException($"Unable to execute SQL statement on Databricks. Status code: {httpResponse.StatusCode}");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None, };
        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse, settings) ??
                         throw new InvalidOperationException();

        var state = jsonObject["status"]?["state"]?.ToString() ?? throw new InvalidOperationException("Unable to retrieve 'state' from the responseJsonObject");
        if (state != "SUCCEEDED")
        {
            throw new SqlException($"Failed to execute SQL statement: {sqlStatement}. Response: {jsonResponse}");
        }
    }
}
