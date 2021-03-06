# TestCommon Release notes

## Version 3.1.0

- Added property `LogAnalyticsWorkspaceId` to `IntegrationTestConfiguration` to support use of Log Analytics Workspace.

## Version 3.0.1

- Bump version as part of pipeline change

## Version 3.0.0

- Upgrade solution projects from .NET 5 to .NET 6

## Version 2.4.4

- Added `ObjectAssertionsExtensions` with extension `NotContainNullsOrEmptyEnumerables` which Recursively checks all properties if any are null.

## Version 2.3.4

- Bumped patch version as pipeline file was updated.

## Version 2.3.3

- Added backend app object id to `AzureB2CSettings`.

## Version 2.3.2

- Bumped patch version as pipeline file was updated.

## Version 2.3.1

- Released by mistake. Content is similar to 2.2.1.

## Version 2.3.0

- Added feature to override default LocalDB SQL server connection string

## Version 2.2.1

- Bumped patch version as pipeline file was updated.

## Version 2.2.0

- Added backend service principal object id to `AzureB2CSettings`.

## Version 2.1.1

- Bumped patch version as pipeline file was updated.

## Version 2.1.0

- Extended `IntegrationTestConfiguration` to support using Azure AD B2C.

## Version 2.0.0

- `ITopicResourceBuilder.AddSubscription()` is extended with the possibility to set the `requiresSession` parameter for `CreateSubscriptionOptions`.
- `ServiceBusResourceProvider.BuildQueue()` parameter `requireSession` is renamed to `requiresSession` to comply with the underlying method. This is a breaking change

## Version 1.4.1

- Update build pipeline, which forced republishing the same package content.

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
