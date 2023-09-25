# Version 3.0.0

In this new version we
* added Health checks for the Databricks SQL Statement Execution API
* added HealthChecks for the Databricks Jobs API
* added `JobsApiClient`as a client for the Databricks Jobs API

Register the `IJobsApiClient` to the service collection using `JobsExtensions.AddDatabricksJobs()`

```c#
private static void AddDatabricksJobs(IServiceCollection services)
{   
    var options = new DatabricksOptions();
    configuration.GetSection("DatabricksOptions").Bind(options);

    services.AddDatabricksJobs(options);
}
```
