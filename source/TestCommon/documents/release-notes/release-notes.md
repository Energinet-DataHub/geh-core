# TestCommon Release notes

## Version 1.1.0

- Extended control of starting an Azure Function App through use of the `FunctionAppHostManager`:
  - Specify which functions to load using `FunctionAppHostSettings.Functions`.
  - Set environment variables for the process using `FunctionAppHostSettings.ProcessEnvironmentVariables`.
  - Write host process id to output when started, so we know which process to attach debugger to if needed.
- Updated default values for `FunctionAppHostSettings` to reduce the necessary settings in  `functionapphost.settings.json`.
- Implemented `FunctionAppHostManagerExtensions`:
  - `CheckIfFunctionWasExecuted`
  - `TriggerFunctionAsync`

## Version 1.0.1

- Ensure `AzuriteManager` does not fail when Azure Storage Emulator is not installed on machine.

## Version 1.0.0

- Implemented `ServiceBusResourceProvider`. See [Resource providers](../functionapp-testcommon.md#resource-providers).
- Implemented `IntegrationTestConfiguration` to support using Integration Test environment. See [Integration Test environment](../functionapp-testcommon.md#integration-test-environment).

## Version 0.0.1

- Preparing packages for initial release.
