# Health Checks

Guidelines on implementing health checks for Azure Function App's and ASP.NET Core Web API's.

> For a full implementation, see [Wholesale](https://github.com/Energinet-DataHub/opengeh-wholesale) repository/subsystem.

## Overview

- [Introduction](#introduction)
- Implementation
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

## Introduction

Health checks should be used to check critical components of our applications.

In DataHub, any application implemented as an Azure Function App or an ASP.NET Core Web API should implement health checks.

Each application should expose two health checks endpoints:

- **liveness**: for determining if the application is live (like a _ping_).
- **readiness**: for determining if the application is ready to serve requests.

The **readiness** check should validate that the application can reach critical dependencies, e.g. a SQL Server database or a Service Bus queue.

The **liveness** check should be used when validating critical application dependencies. E.g if App-A depends on App-B, then App-A should check its dependency with App-B by calling the _liveness_ endpoint of App-B. It is imperative that applications does not call each others _readiness_ check as this could cause an endless loop.

### Calling liveness of other service

We have implemented `AddServiceHealthCheck()` to support calling liveness health check of other services. See examples under implementation for usage.

### Health Checks UI compatible response

The health checks returns a response that is compatible with the use of the [Health Checks UI](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#HealthCheckUI). This is a JSON format that allows callers to drill down into each specific health check and determine its status.

### Application version information

The _liveness_ endpoint also returns the source version information in the `description` field of the JSON, for any DH3 application that was build using the standard DH3 CI workflows. This version information is extracted from the executing applications `AssemblyInformationalVersion` property.

## Azure Functions App

After following the guidelines below, the health checks endpoints will be:

- _liveness_: `api/monitor/live`
- _readiness_: `api/monitor/ready`

### Preparing an Azure Function App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.FunctionApp`

1) Register Health Checks handling in the _ConfigureServices()_ method in Program.cs:

   ```cs
    .ConfigureServices(services =>
    {
        services.AddHealthChecksForIsolatedWorker();
    })
   ```

1) Create a new class file as `Monitor\HealthCheckEndpoint.cs` with the following content:

   ```cs
    public class HealthCheckEndpoint
    {
        public HealthCheckEndpoint(IHealthCheckEndpointHandler healthCheckEndpointHandler)
        {
            EndpointHandler = healthCheckEndpointHandler;
        }

        private IHealthCheckEndpointHandler EndpointHandler { get; }

        [Function("HealthCheck")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "monitor/{endpoint}")]
            HttpRequestData httpRequest,
            string endpoint)
        {
            return EndpointHandler.HandleAsync(httpRequest, endpoint);
        }
    }
   ```

### Add health checks for Azure Function App dependencies

See [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#health-checks) for a number of health checks supported through NuGet packages. Even though they are implemented for ASP.NET Core, they also work for Azure Functions.

1) Add additional health checks using `AddHealthChecks()`. See an example below.

   ```cs
   services.AddHealthChecks()
       // Example where our application has a dependency to a SQL Server database
       .AddSqlServer(
           name: "MyDb",
           connectionString: Configuration.GetConnectionString("MyDbConnectionString"))
       // Example where our application has a dependency to another service
       .AddServiceHealthCheck("service-name", <url-to-ping>);
   ```

## ASP.NET Core Web API

After following the guidelines below, the health checks endpoints will be:

- _liveness_: `monitor/live`
- _readiness_: `monitor/ready`

### Preparing a Web App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.WebApp`

1) Add the following to the _services registration_ section of Program.cs (minimal hosting model):

   ```cs
   builder.Services.AddHealthChecksForWebApp();
   ```

1) Add the following to _configuration_ section of Program.cs (minimal hosting model):

   ```cs
   app.MapLiveHealthChecks();
   app.MapReadyHealthChecks();
   ```

### Add health checks for Web App dependencies

See [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#health-checks) for a number of health checks supported through NuGet packages.

1) Add additional health checks using `AddHealthChecks()`. See an example below.

   ```cs
   services.AddHealthChecks()
       // Example where our application has a dependency to a SQL Server database
       .AddSqlServer(
           name: "MyDb",
           connectionString: Configuration.GetConnectionString("MyDbConnectionString"))
       // Example where our application has a dependency to another service
       .AddServiceHealthCheck("service-name", <url-to-ping>);
   ```
