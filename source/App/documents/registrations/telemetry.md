# Telemetry and logging to Application Insights

Guidelines for Azure Function App's and ASP.NET Core Web API's on configuring logging of telemetry to Application Insights.

> For a full implementation, see [Wholesale](https://github.com/Energinet-DataHub/opengeh-wholesale) repository/subsystem.

## Overview

- [Introduction](#introduction)
- Implementation
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

## Introduction

Telemetry should be emitted to Application Insights so we can analyze the performance and usage of our applications.

In DataHub, any application implemented as an Azure Function App or an ASP.NET Core Web API should use our guidelines for configuration so that:

- Telemetry types to monitor the execution of the application are automatically collected (data model types: request, exception, dependency).
- Custom telemetry can be emitted using the standard .NET Core types `ILogger` and `ILogger<T>` (data model type: trace).
- The default and categories log level of custom telemetry can be configured in application settings.
- Telemetry has `ApplicationVersion` property set to the value in `AssemblyInformationalVersion` of the executing assembly.
- Telemetry are enriched with custom properties, like the `Subsystem` property.

See also [Application Insights telemetry data model](https://learn.microsoft.com/en-us/azure/azure-monitor/app/data-model-complete).

## Azure Functions App

After following the guidelines below, the default and categories log level of custom telemetry can be configured in application settings.

### Preparing an Azure Function App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.FunctionApp`

1) Register Application Insights in the _ConfigureServices()_ method in Program.cs:

   ```cs
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsForIsolatedWorker(subsystemName: "MySubsystem");
    })
   ```

1) Register logging configuration in the _ConfigureLogging()_ method in Program.cs:

   ```cs
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext.Configuration);
    });
   ```

1) Perform configuration in application settings

   Example `local.settings.json` with focus on settings for Application Insights.

   It is possible to add additional log level configuration per logging category following the example for `Energinet.DataHub.Core`.

   ```json
   {
     "Values": {
       // Connection string
       "APPLICATIONINSIGHTS_CONNECTION_STRING": "<connection string>",
       // Default log level for Application Insights
       "Logging__ApplicationInsights__LogLevel__Default": "Information",
       // Log level for category (code in Energinet.DataHub.Core). 
       // NOTICE category filters are case-insensitive, so the given namespace-filter doesn't have to match the actual casing of the namespace.
       "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core": "Information",
     }
   }
   ```

## ASP.NET Core Web API

After following the guidelines below, the default and categories log level of custom telemetry can be configured in application settings.

### Preparing a Web App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.WebApp`

1) Add the following to Program.cs (minimal hosting model):

   ```cs
   builder.Services.AddApplicationInsightsForWebApp(subsystemName: "MySubsystem");
   ```

1) Perform configuration in application settings

   Example `appsettings.json` with focus on settings for Application Insights.

   It is possible to add additional log level configuration per logging category following the example for `Microsoft.AspNetCore`.

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         // Log level for category (code in Microsoft.AspNetCore)
         "Microsoft.AspNetCore": "Warning"
       },
       // Default log level for Application Insights
       "ApplicationInsights": {
         "LogLevel": {
           "Default": "Information"
         }
       }
     },
     // Connection string
     "APPLICATIONINSIGHTS_CONNECTION_STRING": "<connection string>",
   }
   ```
