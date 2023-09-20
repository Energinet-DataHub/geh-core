# Development notes for App

Notes regarding the development of the NuGet package bundle `App`.

The bundle contains the following packages:

* `Energinet.DataHub.Core.App.Common`
* `Energinet.DataHub.Core.App.Common.Abstractions`
* `Energinet.DataHub.Core.App.Common.Security`
* `Energinet.DataHub.Core.App.FunctionApp`
* `Energinet.DataHub.Core.App.FunctionApp.SimpleInjector`
* `Energinet.DataHub.Core.App.WebApp`
* `Energinet.DataHub.Core.App.WebApp.SimpleInjector`

The packages contain types commonly used by domain teams when implementing Azure Function App's and Web App's.

> Also read the general [development.md](../../../documents/development.md) as is contains information that is relevant for all NuGet package bundles.

## Setup local environment

First, we must ensure we have followed any general setup of the developer environment for the Energinet DataHub.

Secondly, we must ensure we obey the following [prerequisites](../../TestCommon/documents/functionapp-testcommon.md#prerequisites).

### Dependencies to live Azure resources

The `ExampleHost.FunctionApp.Tests` depends on live Azure resources like Service Bus end Application Insights. We cannot mock, or install these locally, so we have to use actual instances.

To be able to use the Azure resources prepared in the Integration Test environment, developers must do the following:

* Copy of the `integrationtest.local.settings.sample.json` file into `integrationtest.local.settings.json`
* Update `integrationtest.local.settings.json` with information matching the Integration Test environment.
