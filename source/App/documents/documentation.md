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

For a full implementation, see [Program.cs](https://github.com/Energinet-DataHub/opengeh-wholesale/blob/main/source/dotnet/wholesale-api/Orchestrations/Program.cs) for Wholesale Orchestrations application.

Features of the example:

- Demonstrates the configuration of an Azure Function using the equivalent to the _minimal hosting model_.
- Registers telemetry to Application Insights and configures the default log level for Application Insights to "Information". Telemetry emitted from the worker has:
    - `ApplicationVersion` property set to the value in `AssemblyInformationalVersion` of the worker assembly.
    - Custom property `Subsystem` set to a configured value
- Registers health checks "live", "ready" and "status" endpoints:
    - Requires the `Monitor\HealthCheckEndpoint.cs` as documented under [Health Checks](./registrations/health-checks.md#preparing-an-azure-function-app-project).
    - Information returned from call to "live" endpoint contains same `AssemblyInformationalVersion` as logged to Application Insights.
- Registers Noda Time to its default time zone "Europe/Copenhagen".
- Registers JWT bearer authentication as documented under [JWT Security](./registrations/authorization.md).
- Register feature management with support for feature flags in app settings and Azure App Configuration.

Preparing an Azure Function App project:

1) Install this NuGet package: `Energinet.DataHub.Core.App.FunctionApp`

1) Add `Program.cs` with the following content

   WARNING: The order in which the `ConfigureXxx` methods are called are important.

   ```cs
   var host = new HostBuilder()
       .ConfigureServices((context, services) =>
       {
           // Common
           services.AddApplicationInsightsForIsolatedWorker(subsystemName: "MySubsystem");
           services.AddHealthChecksForIsolatedWorker();

           // Http => Authentication
           services
             .AddJwtBearerAuthenticationForIsolatedWorker(context.Configuration)
             .AddUserAuthenticationForIsolatedWorker<SubsystemUser, SubsystemUserProvider>();

           // Would typically be registered within functional module registration methods instead of here.
           services.AddNodaTimeForApplication();

           // Feature management
           services
               .AddAzureAppConfiguration()
               .AddFeatureManagement();
       })
       .ConfigureFunctionsWebApplication(builder =>
       {
           // Feature management
           //  * Enables middleware that handles refresh from Azure App Configuration
           builder.UseAzureAppConfiguration();

           // Http => Authorization
           builder.UseFunctionsAuthorization();
           // Http => Authentication
           builder.UseUserMiddlewareForIsolatedWorker<SubsystemUser>();
       })
       .ConfigureAppConfiguration((context, configBuilder) =>
       {
           // Feature management
           //  * Configure load/refresh from Azure App Configuration
           configBuilder.AddAzureAppConfigurationForIsolatedWorker();
       })
       .ConfigureLogging((hostingContext, logging) =>
       {
           logging.AddLoggingConfigurationForIsolatedWorker(hostingContext.Configuration);
       })
       .Build();

   host.Run();
   ```

1) Implement `SubsystemUser` and `SubsystemUserProvider` accordingly.

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
       // Authentication
       "UserAuthentication__MitIdExternalMetadataAddress": "<metadata address>",
       "UserAuthentication__ExternalMetadataAddress": "<metadata address>",
       "UserAuthentication__InternalMetadataAddress": "<metadata address>",
       "UserAuthentication__BackendBffAppId": "<app id>",
       // Feature management (Azure App Configuration)
       "AzureAppConfiguration__Endpoint": "<azure app config endpoint>",
     }
   }

   ```

### ASP.NET Core Web API

For a full implementation, see [Program.cs](https://github.com/Energinet-DataHub/opengeh-wholesale/blob/main/source/dotnet/wholesale-api/WebApi/Program.cs) for Wholesale Web API application.

Features of the example:

- Demonstrates the configuration of a _controller based API_ using the _minimal hosting model_.
- Registers telemetry to Application Insights and configures the default log level for Application Insights to "Information". Telemetry emitted from the application has:
    - `ApplicationVersion` property set to the value in `AssemblyInformationalVersion` of the executing assembly.
    - Custom property `Subsystem` set to a configured value.
- Registers health checks "live", "ready" and "status" endpoints:
    - Information returned from call to "live" endpoint contains same `AssemblyInformationalVersion` as logged to Application Insights.
- Registers Noda Time to its default time zone "Europe/Copenhagen".
- Registers API Versioning and Swagger UI to the default API version `v1`.
- Registers JWT bearer authentication as documented under [JWT Security](./registrations/authorization.md).
- Register feature management with support for feature flags in app settings and Azure App Configuration.

Preparing a Web App project:

1) Install this NuGet package: `Energinet.DataHub.Core.App.WebApp`

1) Add `Program.cs` with the following content

   WARNING: The order of middleware enabling methods can be important. The `UseAzureAppConfiguration()` should be called before mapping endpoints.

   ```cs
   var builder = WebApplication.CreateBuilder(args);

   /*
   // Configuration
   */

   // Feature management
   //  * Configure load/refresh from Azure App Configuration
   builder.Configuration.AddAzureAppConfigurationForWebApp(builder.Configuration);

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
   builder.Services.AddApiVersioningForWebApp(defaultVersion: new ApiVersion(1, 0));

   // => Authentication/authorization
   builder.Services
       .AddJwtBearerAuthenticationForWebApp(builder.Configuration)
       .AddUserAuthenticationForWebApp<SubsystemUser, SubsystemUserProvider>()
       .AddPermissionAuthorizationForWebApp();

   // Feature management
   services
       .AddAzureAppConfiguration()
       .AddFeatureManagement();

   var app = builder.Build();

   /*
   // Configure the HTTP request pipeline.
   */

   // Feature management
   //  * Enables middleware that handles refresh from Azure App Configuration
   app.UseAzureAppConfiguration();

   app.UseRouting();
   app.UseSwaggerForWebApp();
   app.UseHttpsRedirection();

   // Authentication/authorization
   app.UseAuthentication();
   app.UseAuthorization();
   app.UseUserMiddlewareForWebApp<SubsystemUser>();

   // Health Checks
   app.MapLiveHealthChecks();
   app.MapReadyHealthChecks();
   app.MapStatusHealthChecks();

   app.Run();

   // Enable testing
   public partial class Program { }
   ```

1) Implement `SubsystemUser` and `SubsystemUserProvider` accordingly.

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
     // Authentication
     "UserAuthentication": {
       "MitIdExternalMetadataAddress": "<metadata address>",
       "ExternalMetadataAddress": "<metadata address>",
       "InternalMetadataAddress": "<metadata address>",
       "BackendBffAppId": "<app id>"
     }
     // Feature management (Azure App Configuration)
     "AzureAppConfiguration": {
       "Endpoint": "<azure app config endpoint>"
     }
   }
   ```
