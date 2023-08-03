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
    services.AddDatabricks(options.WarehouseId, options.WorkspaceToken, options.WorkspaceUrl);
}
```

Example of how to use the SQL Statement Execution client.

```c#
[HttpGet]
public async Task<IActionResult> GetAsync()
{
    var sqlQuery = GenerateQuery();
    var resultList = new List<TestModel>();

    await foreach (var row in _sqlStatementClient.ExecuteAsync(sqlQuery)) {
        var testModel = new TestModel(row["column1"]);
        resultList.Add(testModel)
    }

    return Ok(resultList);
}

private string GenerateQuery()
{
    return $"SELECT column1 FROM database.table";
}
```
