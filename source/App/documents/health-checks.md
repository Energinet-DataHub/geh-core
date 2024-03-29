# Health Checks Documentation

Guidelines on implementing health checks for Azure Function App's and ASP.NET Core Web API's.

> For a full implementation, see [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

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

### Health Checks UI compatible response

The health checks returns a response that is compatible with the use of the [Health Checks UI](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#HealthCheckUI). This is a JSON format that allows callers to drill down into each specific health check and determine its status.

The _liveness_ endpoint also returns the source version information in the `description` field of the JSON, for any DH3 application that was build using the standard DH3 CI workflows.

## Azure Functions App

After following the guidelines below, the health checks endpoints will be:

- _liveness_: `api/monitor/live`
- _readiness_: `api/monitor/ready`

### Preparing an Azure Function App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.FunctionApp`

1) Add the following to a _ConfigureServices()_ method in Program.cs:

   ```cs
    // Health check
    serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
    serviceCollection.AddHealthChecks()
        .AddLiveCheck();
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

1) Allow anonymous access to the health checks endpoints.

    > In the `Charges` domain this is handled using the `JwtTokenWrapperMiddleware`.

### Add health checks for Azure Function App dependencies

See [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#health-checks) for a number of health checks supported through NuGet packages. Even though they are implemented for ASP.NET Core, they also work for Azure Functions.

We have implemented `AddServiceHealthCheck()` to support calling liveness health check of other services.

1) Add additional health checks after the call to `AddLiveCheck()`. See an example below.

   ```cs
    services.AddHealthChecks()
        .AddLiveCheck()
        // Example where our application has a dependency to a SQL Server database
        .AddSqlServer(
            name: "ChargeDb",
            connectionString: Configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString))
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

1) Add the following to a _ConfigureServices()_ method in Program.cs:

   ```cs
    // Health check
    services.AddHealthChecks()
        .AddLiveCheck();
   ```

1) Add the following to a _Configure()_ method in Program.cs:

   ```cs
    app.UseEndpoints(endpoints =>
    {
        ...

        // Health check
        endpoints.MapLiveHealthChecks();
        endpoints.MapReadyHealthChecks();
    });
   ```

### Add health checks for Web App dependencies

See [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#health-checks) for a number of health checks supported through NuGet packages.

We have implemented `AddServiceHealthCheck()` to support calling liveness health check of other services.

1) Add additional health checks after the call to `AddLiveCheck()`. See an example below.

   ```cs
    services.AddHealthChecks()
        .AddLiveCheck()
        // Example where our application has a dependency to a SQL Server database
        .AddSqlServer(
            name: "ChargeDb",
            connectionString: Configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString))
        // Example where our application has a dependency to another service
        .AddServiceHealthCheck("service-name", <url-to-ping>);
   ```
