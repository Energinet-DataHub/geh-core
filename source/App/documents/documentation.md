# Documentation

Documentation of the NuGet package bundle `App`.

The `App` package bundle contains common functionality for Azure Functions and ASP.Net Core Web API's implemented as dependency injection extensions, extensibility types etc.

Using the package bundle enables an easy opt-in/opt-out pattern of services during startup, for a typical DataHub application.

## Overview

- [Quick start for application startup](#quick-start-for-application-startup)
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

- Detailed walkthrough per subject
    - [Health Checks](./registrations/health-checks.md)
    - [JWT Security](./registrations/authorization.md)
    - [Noda Time](./registrations/noda-time.md)
    - [Swagger and Api versioning](./registrations/swagger-api-version.md)
    - [Telemetry and logging to Application Insights](./registrations/telemetry.md)

- [Development notes for App](development.md)

## Quick start for application startup

In the following we show a simple example per application type, of how to use all the typical registrations during startup.

For a detailed documentation per registration type, see the walkthroughs listed in the [Overview](#overview).

### Azure Functions App

For a full implementation, see [Program.cs](https://github.com/Energinet-DataHub/opengeh-wholesale/blob/main/source/dotnet/wholesale-api/Orchestration/Program.cs) for Wholesale Orchestration application.

Features of the example:

- Demonstrates the configuration of an Azure Function using the equivalent to the _minimal hosting model_.
- Registers telemetry to Application Insights and configures the default log level for Application Insights to "Information".
- Registers health checks "live" and "readiness" endpoints. Requires the `Monitor\HealthCheckEndpoint.cs` as documented under [Health Checks](./registrations/health-checks.md#preparing-an-azure-function-app-project).
- Registers Noda Time to its default time zone "Europe/Copenhagen".

Preparing an Azure Function App project:

1) Install this NuGet package: `Energinet.DataHub.Core.App.FunctionApp`

1) Add `Program.cs` with the following content

   ```cs
   var host = new HostBuilder()
       .ConfigureFunctionsWorkerDefaults()
       .ConfigureServices((context, services) =>
       {
           // Common
           services.AddApplicationInsightsForIsolatedWorker(subsystemName: "MySubsystem");
           services.AddHealthChecksForIsolatedWorker();

           // Would typically be registered within functional module registration methods instead of here.
           services.AddNodaTimeForApplication();
       })
       .ConfigureLogging((hostingContext, logging) =>
       {
           logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
       })
       .Build();

   host.Run();

   ```

1) Perform configuration in application settings

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       // Azure Function
       "AzureWebJobsStorage": "<connection string>",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       // Application Insights
       "APPLICATIONINSIGHTS_CONNECTION_STRING": "<connection string>",
       // Logging
       // => Default log level for Application Insights
       "Logging__ApplicationInsights__LogLevel__Default": "Information",
     }
   }

   ```

## ASP.NET Core Web API

For a full implementation, see [Program.cs](https://github.com/Energinet-DataHub/opengeh-wholesale/blob/main/source/dotnet/wholesale-api/WebApi/Program.cs) for Wholesale Web API application.

Features of the example:

- Demonstrates the configuration of a _controller based API_ using the _minimal hosting model_.
- Registers telemetry to Application Insights and configures the default log level for Application Insights to "Information".
- Registers health checks "live" and "readiness" endpoints.
- Registers Noda Time to its default time zone "Europe/Copenhagen".
- Registers API Versioning and Swagger UI to the default API version `v1`.

Preparing a Web App project:

1) Install this NuGet package: `Energinet.DataHub.Core.App.WebApp`

1) Add `Program.cs` with the following content

   ```cs
   var builder = WebApplication.CreateBuilder(args);

   /*
   // Add services to the container.
   */

   // Common
   builder.Services.AddApplicationInsightsForWebApp(subsystemName: "MySubsystem");
   builder.Services.AddHealthChecksForWebApp();

   // Would typically be registered within functional module registration methods instead of here.
   builder.Services.AddNodaTimeForApplication();

   // Http channels
   builder.Services.AddControllers();

   // => Open API generation
   builder.Services.AddSwaggerForWebApp(Assembly.GetExecutingAssembly(), swaggerUITitle: "My Web API");

   // => API versioning
   builder.Services.AddApiVersioningForWebApp(new ApiVersion(1, 0));

   // => Authentication/authorization
   // TODO: Add "simple" example registration

   var app = builder.Build();

   /*
   // Configure the HTTP request pipeline.
   */

   app.UseRouting();
   app.UseSwaggerForWebApp();
   app.UseHttpsRedirection();

   // Authentication/authorization
   // TODO: Add "simple" example registration

   // Health Checks
   app.MapLiveHealthChecks();
   app.MapReadyHealthChecks();

   app.Run();

   // Enable testing
   public partial class Program { }
   ```

1) Perform configuration in application settings

   ```json
   {
     // Logging
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       },
       // => Default log level for Application Insights
       "ApplicationInsights": {
         "LogLevel": {
           "Default": "Information"
         }
       }
     },
     // Application Insights
     "APPLICATIONINSIGHTS_CONNECTION_STRING": "<connection string>",
   }
   ```
