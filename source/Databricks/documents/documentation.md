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
    var sqlQuery = "SELECT column1 FROM database.table";
    var resultList = new List<TestModel>();

    await foreach (var row in _databricksSqlStatementClient.ExecuteAsync(sqlQuery)) {
        var testModel = new TestModel(row["column1"]);
        resultList.Add(testModel)
    }

    return Ok(resultList);
}
```

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
    services.Configure<DatabricksJobsOptions>(
                configuration.GetSection(DatabricksJobsOptions.DatabricksOptions));
    
    services.AddDatabricksJobs();
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
private static void AddJobsApiHealthChecks(IServiceCollection serviceCollection, IConfiguration configuration)
{
    services.Configure<DatabricksJobsOptions>(
                configuration.GetSection(DatabricksJobsOptions.DatabricksOptions));
    
    serviceCollection.AddHealthChecks()
        .AddLiveCheck()
        .AddDatabricksJobsApiHealthCheck();
}
```
