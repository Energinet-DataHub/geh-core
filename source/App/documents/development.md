# Development notes for App

Notes regarding the development of the NuGet package bundle `App`.

The bundle contains the following packages:

* `Energinet.DataHub.Core.App.Common`
* `Energinet.DataHub.Core.App.Common.Abstractions`
* `Energinet.DataHub.Core.App.FunctionApp`
* `Energinet.DataHub.Core.App.WebApp`

The packages contain types commonly used by subsystem teams when implementing Azure Function App's and Web App's.

> Also read the general [development.md](../../../docs/development.md) as is contains information that is relevant for all NuGet package bundles.

## ExampleHost applications

To be able to develop effeciently, especially with regards to dependency injection extensions and other types of startup configuration, we have implemented a number of `ExampleHost` applications.

These allows us to easily debug, as well as implement integration tests, for verifying the runtime behaviour of our code running in the given type of application. This is important as unit tests of that kind of code doesn't offer sufficient confidence as dependencies outside our control changes all the time and a small change might cause our functionality to break.

### ExampleHost.FunctionApp01 and ExampleHost.FunctionApp02

`ExampleHost.FunctionApp01` is used from integration tests located in `ExampleHost.FunctionApp.Tests` for verifying:

* Telemetry or Application Insights configuration. It depends on `ExampleHost.FunctionApp02` for the verification scenario.
* Health Checks configuration.
* Authentication configuration.

### ExampleHost.WebApi01 and ExampleHost.WebApi02

`ExampleHost.WebApi01` is used from integration tests located in `ExampleHost.WebApi.Tests` for verifying:

* Telemetry or Application Insights configuration. It depends on `ExampleHost.WebApi02` for the verification scenario.
* Health Checks configuration.

### ExampleHost.WebApi03

`ExampleHost.WebApi03` is used from integration tests located in `ExampleHost.WebApi.Tests` for verifying:

* Authentication and authorization configuration.

## Setup local environment

First, we must ensure we have followed any general setup of the developer environment for the Energinet DataHub.

Secondly, we must ensure we obey the following [prerequisites](../../TestCommon/documents/functionapp-testcommon.md#prerequisites).

### Dependencies to live Azure resources

The `ExampleHost.FunctionApp.Tests` and `ExampleHost.WebApi.Tests` depends on live Azure resources like Application Insights and Service Bus. We cannot mock, or install these locally, so we have to use actual instances.

To be able to use the Azure resources prepared in the Integration Test environment, developers must do the following per test project:

* Copy of the `integrationtest.local.settings.sample.json` file into `integrationtest.local.settings.json`
* Update `integrationtest.local.settings.json` with information matching the Integration Test environment.
