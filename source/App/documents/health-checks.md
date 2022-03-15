# Health Checks Documentation

Guidelines on implementing health checks for Function App's and ASP.NET Core Web API's.

> For a full implementation, see [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

## Overview

- [Introduction](#introduction)
- Implementation
  - [Azure Functions App](#azure-functions-app)
  - [ASP.NET Core Web API](#aspnet-core-web-api)

## Introduction



## Azure Functions App

### Preparing an Azure Function App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.FunctionApp`

1) Add the following to a *ConfigureServices()* method in Program.cs:

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

### Preparing a Web App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.WebApp`

1) Add the following to a *ConfigureServices()* method in Program.cs:

   ```cs
    // Health check
    services.AddHealthChecks()
        .AddLiveCheck();
   ```

1) Add the following to a *Configure()* method in Program.cs:

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
