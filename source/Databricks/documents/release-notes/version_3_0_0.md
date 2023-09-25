# Version 3.0.0

In this new version we have added `JobsApiClient`as a client for the Databricks Jobs API.

Register the `IJobsApiClient` to the service collection using `JobsExtensions.AddDatabricksJobs()`

```c#
private static void AddDatabricksJobs(IServiceCollection services, IConfiguration configuration)
{   
    var options = new DatabricksOptions();
    configuration.GetSection("DatabricksOptions").Bind(options);

    services.AddDatabricksJobs(options);
}
```
