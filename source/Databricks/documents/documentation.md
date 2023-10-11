# Databricks Documentation

## SQL Statement Execution

This project contains a client for the Databricks SQL Statement Execution API. The client is a wrapper around the Databricks REST API. The client is used to execute SQL statements on a Databricks cluster.

The implementation uses solely `disposition=EXTERNAL_LINKS` despite that inlining is simpler. This is because inlining has a limit of 16MB, which isn't sufficient for all use cases.

### Usage

Install `Energinet.DataHub.Core.Databricks.SqlStatementExecution` package.

Example of how to setup the Databricks Sql Statement Execution in `startup.cs`.

```c#
private static void AddDatabricksSqlStatementExecution(IServiceCollection services, IConfiguration configuration)
{   
    services.Configure<DatabricksSqlStatementOptions>(
                configuration.GetSection(DatabricksSqlStatementOptions.DatabricksOptions));
    
    services.AddDatabricksSqlStatementExecution();
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
        SqlStatementParameter.Create("my_date", "26-02-1980", "DATE"),
    };
    var resultList = new List<TestModel>();

    await foreach (var row in _databricksSqlStatementClient.ExecuteAsync(sqlQuery, parameters)) {
        var testModel = new TestModel(row["my_name"], row["my_date"]);
        resultList.Add(testModel)
    }

    return Ok(resultList);
}
```

Example of how to stream data with the SQL Statement Execution client.

```c#
[HttpGet]
public async Task<IActionResult> StreamAsync()
{
    var sqlQuery = "SELECT * FROM my_table WHERE name = :my_name AND date = :my_date";
    var parameters = new List<SqlStatementParameter>
    {
        SqlStatementParameter.Create("my_name", "Sheldon Cooper"),
        SqlStatementParameter.Create("my_date", "26-02-1980", "DATE"),
    };
    var resultList = new List<TestModel>();
    
    await foreach (var row in _databricksSqlStatementClient.StreamAsync(sqlQuery, parameters))
    {
        var myName = row[0];
        var myDate = row[1];
        var testModel = new TestModel(myName, myDate);
        resultList.Add(testModel)
    }

    return Ok(resultList);
}
```

Notice, if a type is given in the `SqlStatementParameter` Create method, Databricks SQL Statement Execution API will perform type checking on the parameter value. But no functional difference. See [Databricks documentation](https://docs.databricks.com/api/workspace/statementexecution/executestatement) for more information.

### Health checks

The package contains functionality to do health checks of the status of the Databricks Sql Statement Execution API.

Example of how to setup the Databricks Sql Statement Execution health check in `startup.cs`.

```c#
private static void AddSqlStatementApiHealthChecks(IServiceCollection serviceCollection, IConfiguration configuration)
{
    services.Configure<DatabricksSqlStatementOptions>(
                configuration.GetSection(DatabricksSqlStatementOptions.DatabricksOptions));
    
    serviceCollection.AddHealthChecks()
        .AddLiveCheck()
        .AddDatabricksSqlStatementApiHealthCheck();
}
```

## Jobs

This project contains a client for the Databricks Jobs API. The client is a wrapper around the Databricks Jobs REST API. The client is used to execute Jobs on a Databricks cluster.

### Usage

Install `Energinet.DataHub.Core.Databricks.Jobs` package.

Example of how to setup the Databricks in `startup.cs`.

```c#
private static void AddDatabricksJobs(IServiceCollection services, IConfiguration configuration)
{   
    services.AddDatabricksJobs(configuration);
}
```

Example of how to use the Jobs client.

```c#
[HttpGet]
public async Task<Job> GetAsync()
{
    var jobs = await _client.Jobs.List().ConfigureAwait(false);
    var exampleJob = jobs.Jobs.Single(j => j.Settings.Name == "ExampleJob");
    return await _client.Jobs.Get(exampleJob.JobId).ConfigureAwait(false);
}
```

### Health checks

The package contains functionality to do health checks of the status of the Databricks Jobs API.

Example of how to setup the Databricks Jobs health check in `startup.cs`.

```c#
private static void AddJobsApiHealthChecks(IServiceCollection serviceCollection)
{
    serviceCollection.AddHealthChecks()
        .AddLiveCheck()
        .AddDatabricksJobsApiHealthCheck();
}
```
