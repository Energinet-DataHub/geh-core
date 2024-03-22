# Azure App Common Documentation

A library containing common functionality for Azure Functions and ASP.Net Core Web API's.

## Overview

For the following subjects we have implemented dependency injection extensions, extensibility types etc. to enable an easy opt-in/out pattern during startup, for a typical DataHub application. Each subject is further described in the added links.

For a cheat sheet for application startup, see the [Quick guide for application startup](#quick-guide-for-application-startup).

- Monitoring
    - [Health Checks](./registrations/health-checks.md)
- Security
    - [JWT Security](./registrations/authorization.md)
- Telemetry
    - [Telemetry](./registrations/telemetry.md)

## Quick guide for application startup

In the following we show a simple example of using all the registrations at once during startup. The example shows applications using the minimal hosting model.

### Function App

```cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Common infrastructure layer
        services.AddApplicationInsightsForIsolatedWorker("MySubsystem");
        services.AddHealthChecksForIsolatedWorker();

        // Common application layer

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
    })
    .Build();

host.Run();

```

### Web App
