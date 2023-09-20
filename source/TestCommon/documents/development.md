# Development notes for TestCommon

Notes regarding the development of the NuGet package bundle `TestCommon`.

The bundle contains the following packages:

* `Energinet.DataHub.Core.FunctionApp.TestCommon`
* `Energinet.DataHub.Core.TestCommon`

The packages contain reusable types, supporting the development and test of Energinet DataHub components.

> Also read the general [development.md](../../../docs/development.md) as is contains information that is relevant for all NuGet package bundles.

## Setup local environment

First, we must ensure we have followed any general setup of the developer environment for the Energinet DataHub.

Secondly, we must ensure we obey the following [prerequisites](./functionapp-testcommon.md#prerequisites).

### Dependencies to live Azure resources

The `FunctionApp.TestCommon.Tests` depends on live Azure resources like Service Bus end Event Hub. We cannot mock, or install these locally, so we have to use actual instances.

To be able to use the Azure resources prepared in the Integration Test environment, developers must do the following:

* Copy of the `integrationtest.local.settings.sample.json` file into `integrationtest.local.settings.json`
* Update `integrationtest.local.settings.json` with information matching the Integration Test environment.
