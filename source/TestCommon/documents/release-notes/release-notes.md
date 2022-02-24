# TestCommon Release notes

## Version 1.4.0

- Add `AutoMoqDataAttribute` which enables auto mocking of test parameters.
- Add `InlineAutoMoqDataAttribute` which enables the possibility to inject dependencies in tests as parameters.

## Version 1.3.0

- Implemented `FunctionAppHostManager.RestartHostIfChanges(IEnumerable<KeyValuePair<string, string>> environmentVariables)` that only restarts the function app if process environment variables has changed.
- Extended control of starting an Azure Function App through use of the `FunctionAppHostManager`:
  - Specify which host log message to await during startup using `FunctionAppHostSettings.HostStartedEvent`.

## Version 1.2.0

- Extended `IntegrationTestConfiguration` to support using Event Hub.
- Implemented `EventHubResourceProvider`. See [Resource providers](../functionapp-testcommon.md#resource-providers).
- Implemented `EventHubListenerMock`. See [EventHubListenerMock](../eventhublistenermock.md)

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
