# Development notes for TestCommon

Notes regarding the development of the NuGet package bundle `TestCommon`.

The bundle contains the following packages:

* `Energinet.DataHub.Core.FunctionApp.TestCommon`
* `Energinet.DataHub.Core.TestCommon`

The packages contain reusable types, supporting the development and test of Energinet DataHub components.

> Information that is relevant for multiple NuGet package bundles should be written in the general [development.md](../../../documents/development.md).

## Workflows

The `testcommon-bundle-publish.yml` handles test, build, pack and publish of the bundle.

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

For managing the life-cycle of any Azure Service Bus resources, it uses [Squadron](https://github.com/SwissLife-OSS/squadron).

An Azure Service Bus namespace with topics/queues are created on the fly, which requires developers to do the following:

* Copy of the `integrationtest.local.settings.json.sample` file into `integrationtest.local.settings.json`
* Update `integrationtest.local.settings.json` with information that allows the creation of the mentioned Azure Service Bus ressources.
