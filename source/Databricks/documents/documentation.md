# Databricks Documentation

## SQL Statement Execution

The SQL statement execution lets you execute SQL statements to Databricks and returns the result.

### Usage

Install `Energinet.DataHub.Core.Databricks.SqlStatementExecution` package.

Example of how to setup the Databricks in `startup.cs`.

```c#
private static void AddDatabricks(IServiceCollection services, IConfiguration configuration)
{
    var options = new DatabricksOptions();
    configuration.GetSection("DatabricksOptions").Bind(options);
    services.AddDatabricks(options);
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
