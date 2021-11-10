# TestCommon Release notes

## Version 1.1.0

- Support managing multiple Azure Function Apps from the same test fixture.
- Extended control of starting an Azure Function App through use of the `FunctinoAppHostManager`:
  - Specify which functions to load using `FunctionAppHostSettings.Functions`.
  - Set environment variables for the process using `FunctionAppHostSettings.ProcessEnvironmentVariables`.

## Version 1.0.1

- Ensure `AzuriteManager` does not fail when Azure Storage Emulator is not installed on machine.

## Version 1.0.0

- Implemented `ServiceBusResourceProvider`. See [Resource providers](../functionapp-testcommon.md#resource-providers).
- Implemented `IntegrationTestConfiguration` to support using Integration Test environment. See [Integration Test environment](../functionapp-testcommon.md#integration-test-environment).

## Version 0.0.1

- Preparing packages for initial release.
