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

Example of how to use the SQL Statement client.

```c#
[HttpGet]
public async Task<IActionResult> Get()
{
    var sqlQuery = GenerateQuery();
    var testData = await _sqlStatementExecutionClient.GetAsync(sqlQuery, MapModel).ConfigureAwait(false);
    return Ok(testData);
}

private string GenerateQuery()
{
    return $"SELECT column1 FROM database.table";
}

private static TestModel MapModel(List<string> x)
{
    return new TestModel(x[0]);
}
```
