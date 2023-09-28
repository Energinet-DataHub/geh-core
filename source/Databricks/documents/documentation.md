# Databricks Documentation

## SQL Statement Execution

This project contains a client for the Databricks SQL Statement Execution API. The client is a wrapper around the Databricks REST API. The client is used to execute SQL statements on a Databricks cluster.

The implementation uses solely `disposition=EXTERNAL_LINKS` despite that inlining is simpler. This is because inlining has a limit of 16MB, which isn't sufficient for all use cases.

### Usage

Install `Energinet.DataHub.Core.Databricks.SqlStatementExecution` package.

Example of how to setup the Databricks in `startup.cs`.

```c#
private static void AddDatabricks(IServiceCollection services, IConfiguration configuration)
{   
    var options = new DatabricksOptions();
    configuration.GetSection("DatabricksOptions").Bind(options);

    // Option 1
    services.AddDatabricksSqlStatementExecution(options.WarehouseId, options.WorkspaceToken, options.WorkspaceUrl);

    // Option 2
    services.AddDatabricksSqlStatementExecution(options);
}
```

Example of how to use the SQL Statement Execution client.

```c#
[HttpGet]
public async Task<IActionResult> GetAsync()
{
    var sqlQuery = "SELECT * FROM my_table WHERE name = :my_name AND date = :my_date";
    var parameters = new List<SqlStatementParameter>
    {
        SqlStatementParameter.Create("my_name", "Sheldon Cooper"),
        SqlStatementParameter.Create("my_date", "26-02-1980"),
    };
    var resultList = new List<TestModel>();

    await foreach (var row in _databricksSqlStatementClient.ExecuteAsync(sqlQuery, parameters)) {
        var testModel = new TestModel(row["my_name"], row["my_date"]);
        resultList.Add(testModel)
    }

    return Ok(resultList);
}
```
