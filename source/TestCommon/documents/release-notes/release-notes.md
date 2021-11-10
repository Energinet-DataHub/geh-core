# TestCommon Release notes

## Version 1.1.0

- Support managing multiple Azure Function Apps from the same test fixture.
- By use of the `FunctionAppHostSettings.Functions` property, we can control which functions to load in an Azure Function App.

## Version 1.0.1

- Ensure `AzuriteManager` does not fail when Azure Storage Emulator is not installed on machine.

## Version 1.0.0

- Implemented `ServiceBusResourceProvider`. See [Resource providers](../functionapp-testcommon.md#resource-providers).
- Implemented `IntegrationTestConfiguration` to support using Integration Test environment. See [Integration Test environment](../functionapp-testcommon.md#integration-test-environment).

## Version 0.0.1

- Preparing packages for initial release.
