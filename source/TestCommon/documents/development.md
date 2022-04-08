# Development notes for TestCommon

Notes regarding the development of the NuGet package bundle `TestCommon`.

The bundle contains the following packages:

* `Energinet.DataHub.Core.FunctionApp.TestCommon`
* `Energinet.DataHub.Core.TestCommon`

The packages contain reusable types, supporting the development and test of Energinet DataHub components.

> Information that is relevant for multiple NuGet package bundles should be written in the general [development.md](../../../documents/development.md).

## Workflows

### `testcommon-bundle-publish.yml`

This workflow handles test, build, pack and publish of the bundle.

If any of the packages in the bundle has changes, both currently must be updated with regards to version.

Before publishing anything an action verifies that there is no released version existing with the current version number. This is to help detect if we forgot to update the version number in packages.

If the workflow is triggered:

* Manually (`workflow_dispatch`), a prerelease version of the packages are published.
* By `pull_request`, then the packages are not published.
* By `push` to main, the a release version of the packages are published.

## Setup local environment

First, we must ensure we have followed any general setup of the developer environment for the Energinet DataHub.

Secondly, we must ensure we obey the following [prerequisites](./functionapp-testcommon.md#prerequisites).

### Dependencies to live Azure resources

The `FunctionApp.TestCommon.Tests` depends on live Azure resources like Service Bus end Event Hub. We cannot mock, or install these locally, so we have to use actual instances.

To be able to use the Azure resources prepared in the Integration Test environment, developers must do the following:

* Copy of the `integrationtest.local.settings.sample.json` file into `integrationtest.local.settings.json`
* Update `integrationtest.local.settings.json` with information matching the Integration Test environment.
