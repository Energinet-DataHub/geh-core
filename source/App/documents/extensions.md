# Extensions Documentation

## Function App's

Extensions for Function App's.

### Application Insights

To enable end-to-end tracing of requests entering API Management and floating through backend Function App's, register the following during startup.

```csharp
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        // Step 1: Use middleware
        builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
    })
    .ConfigureServices(services =>
    {
        // Step 2: Add services
        services.AddApplicationInsights();
    })
    .Build();
```

## ASP.NET Core Web API's

Extensions for ASP.NET Core Web API's.