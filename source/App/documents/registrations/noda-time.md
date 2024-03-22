# Noda Time

Guidelines for Azure Function App's and ASP.NET Core Web API's on configuring the typical use of NodaTime in DataHub.

> For a full implementation, see [Wholesale](https://github.com/Energinet-DataHub/opengeh-wholesale) repository/subsystem.

## Overview

- [Introduction](#introduction)
- Implementation
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

## Introduction

In DataHub, applications typically use the following from Noda Time:

- `IClock`configured to a `SystemClock.Instance`. Using an interface allows for improved control in tests.
- `DateTimeZone` configured with the time zone "Europe/Copenhagen".

To know more about Noda Time, see [NodaTime Overview](https://nodatime.org/3.1.x/userguide/index).

## Azure Functions App

After following the guidelines below, the default usage of Noda Time in DataHub is configured.

It is possible to configure `DateTimeZone` to another time zone, as demonstrated in available tests.

### Preparing an Azure Function App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.FunctionApp`

1) Register Application Insights in the _ConfigureServices()_ method in Program.cs:

   ```cs
    .ConfigureServices(services =>
    {
        services.AddNodaTimeForApplication();
    })
   ```

## ASP.NET Core Web API

After following the guidelines below, the default usage of Noda Time in DataHub is configured.

It is possible to configure `DateTimeZone` to another time zone, as demonstrated in available tests.

### Preparing a Web App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.WebApp`

1) Add the following to Program.cs (minimal hosting model):

   ```cs
   builder.Services.AddNodaTimeForApplication();
   ```
