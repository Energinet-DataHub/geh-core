# Azure App Common Documentation

A library containing common functionality for Azure Functions and ASP.Net Core Web API's.

## Overview

We have implemented dependency injection extensions, extensibility types etc. to enable an easy opt-in/opt-out pattern during startup, for a typical DataHub application.

- [Quick start for application startup](#quick-start-for-application-startup)
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

- Detailed walkthrough per subject
    - [Health Checks](./registrations/health-checks.md)
    - [JWT Security](./registrations/authorization.md)
    - [Noda Time](./registrations/noda-time.md)
    - [Swagger and Api versioning](./registrations/swagger-api-version.md)
    - [Telemetry and logging to Application Insights](./registrations/telemetry.md)

## Quick start for application startup

In the following we show a simple example per application type, of how to use all the typical registrations during startup.

For a detailed documentation per registration type, see the walkthroughs listed in the [Overview](#overview).

### Azure Functions App

For a full implementation, see [Program.cs](https://github.com/Energinet-DataHub/opengeh-wholesale/blob/main/source/dotnet/wholesale-api/Orchestration/Program.cs) for Wholesale Orchestration application.

Example is showing the Azure Function equivalent to the _minimal hosting model_.

1. Install this NuGet package: `Energinet.DataHub.Core.App.FunctionApp`

1. Add `Program.cs` with the following content

   ```cs
   var host = new HostBuilder()
       .ConfigureFunctionsWorkerDefaults()
       .ConfigureServices((context, services) =>
       {
           // Common
           services.AddApplicationInsightsForIsolatedWorker("MySubsystem");
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

## ASP.NET Core Web API

For a full implementation, see [Program.cs](https://github.com/Energinet-DataHub/opengeh-wholesale/blob/main/source/dotnet/wholesale-api/WebApi/Program.cs) for Wholesale Web API application.

Example is showing a _controller based API_ using the _minimal hosting model_.

1. Install this NuGet package: `Energinet.DataHub.Core.App.WebApp`

1. Add `Program.cs` with the following content

   ```cs
   var builder = WebApplication.CreateBuilder(args);

   /*
   // Add services to the container.
   */

   // Common
   builder.Services.AddApplicationInsightsForWebApp("MySubsystem");
   builder.Services.AddHealthChecksForWebApp();

   // Would typically be registered within functional module registration methods instead of here.
   builder.Services.AddNodaTimeForApplication();

   // Http channels
   builder.Services.AddControllers();

   // => Open API generation
   builder.Services.AddSwaggerForWebApp(Assembly.GetExecutingAssembly());

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

   // Health check
   app.MapLiveHealthChecks();
   app.MapReadyHealthChecks();

   app.Run();

   // Enable testing
   public partial class Program { }
   ```
