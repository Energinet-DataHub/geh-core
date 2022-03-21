# Health Checks Documentation

Guidelines on implementing health checks for Azure Function App's and ASP.NET Core Web API's.

> For a full implementation, see [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

## Overview

- [Introduction](#introduction)
- Implementation
  - [Azure Functions App](#azure-functions-app)
  - [ASP.NET Core Web API](#aspnet-core-web-api)

## Introduction

Each hosted service should expose two health checks endpoints:

- **liveness**: for determining if a service is live (like a _ping_).
- **readyness**: for determining if a service is ready to serve requests.

The **readyness** check should validate that the hosted service can reach all resource dependencies, e.g. a SQL Server database or a Service Bus queue.

If a Service A depends on Service B, then Service A should check its dependency with Service B by calling the **liveness** endpoint of Service B. This is to avoid the risk of having an endless loop.

### Future improvements

#### Verify deployment

To verify a hosted service is working and configured correctly in a given environment, the Continuous Deployment pipeline should call the readyness endpoint just after deployment of the service.

#### Monitor

To continuously monitor the health of a hosted services, we should enable App Services health check to call the readyness endpoint.

See [Monitor App Service instances using Health check](https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check).

## Azure Functions App

After following the guidelines below, the health checks endpoints will be:

- _liveness_: `api/monitor/live`
- _readyness_: `api/monitor/ready`

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

1) Add additional health checks after the call to `AddLiveCheck()`. See an example below.

   ```cs
    services.AddHealthChecks()
        .AddLiveCheck()
        .AddSqlServer(
            name: "ChargeDb",
            connectionString: Configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString));
   ```

## ASP.NET Core Web API

After following the guidelines below, the health checks endpoints will be:

- _liveness_: `monitor/live`
- _readyness_: `monitor/ready`

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

1) Add additional health checks after the call to `AddLiveCheck()`. See an example below.

   ```cs
    services.AddHealthChecks()
        .AddLiveCheck()
        .AddSqlServer(
            name: "ChargeDb",
            connectionString: Configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString));
   ```
