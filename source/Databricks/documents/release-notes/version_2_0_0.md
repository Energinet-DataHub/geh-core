# Version 2.0.0

In this new version we have changed the Client name from `ISqlStatementClient.cs` to `IDatabricksSqlStatementClient`.

Additionally we have changed the way to register the `IDatabricksSqlStatementClient` to the service collection.

There are now two ways to register the Client. Either with the `DatabricksOptions` model or with parameters as show below.

```c#
private static void AddDatabricks(IServiceCollection services, IConfiguration configuration)
{   
    var options = new DatabricksOptions();
    configuration.GetSection("DatabricksOptions").Bind(options);

    // Option 1
    services.AddSqlStatementExecution(options.WarehouseId, options.WorkspaceToken, options.WorkspaceUrl);

    // Option 2
    services.AddSqlStatementExecution(options);
}
```
