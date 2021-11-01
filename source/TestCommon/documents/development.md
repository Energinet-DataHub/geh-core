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

### Azure Service Bus dependency

The `FunctionApp.TestCommon.Tests` have a dependency to an actual Azure Service Bus. We cannot mock, or install a Service Bus locally, so we have to use an actual instance.

The xUnit fixture `ServiceBusListenerMockFixture` is used to orchestrate integration tests for `ServiceBusListenerMock` which depend on Azure Service Bus resources.

In the Integration Test environment we have created a Azure Service Bus namespace, while topics/queues are created on the fly. For managing the life-cycle of topic/queues we use the `ServiceBusResourceProvider`.

To be able to manage topics/queue, developers must do the following:

* Copy of the `integrationtest.local.settings.json.sample` file into `integrationtest.local.settings.json`
* Update `integrationtest.local.settings.json` with information matching the Integration Test environment.
