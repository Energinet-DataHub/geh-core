# TestCommon Release notes

## Version 7.0.1

- Bump Azure.Storage.Blobs
- No functional change.

## Version 7.0.0

- Refactored class `IntegrationTestConfiguration`:
    - Deleted obsolete property `ServiceBusConnectionString`.
    - Deleted property `EventHubConnectionString`.
    - Added property `Credential` and changed the `DefaultAzureCredentialOptions` used when creating `DefaultAzureCredential`.
    - Added property `EventHubNamespaceName`.
    - Added property `EventHubFullyQualifiedNamespace`.
- Refactored class `ServiceBusResourceProvider`:
    - Deleted obsolete constructor with `ConnectionString` parameter.
    - Deleted obsolete property `ConnectionString`.
    - Require `Credential` in constructor; use the new property on `IntegrationTestConfiguration`.
- Refactored class `ServiceBusListenerMock`:
    - Deleted obsolete constructor with `ConnectionString` parameter.
    - Deleted obsolete property `ConnectionString`.
    - Require `Credential` in constructor; use the new property on `IntegrationTestConfiguration`.
- Refactored class `EventHubResourceProvider` to use token-based authentication for accessing Event Hub resources:
    - Deleted constructor with `ConnectionString` parameter.
    - Deleted property `ConnectionString`.
    - Added constructor with `NamespaceName`.
    - Require `Credential` in constructor; use the new property on `IntegrationTestConfiguration`.
    - Added property `NamespaceName`.
    - Added property `FullyQualifiedNamespace`.
- Refactored class `EventHubListenerMock` to use token-based authentication for accessing Event Hub and Storage Account resources:
    - Deleted constructor with `EventHubConnectionString` and `StorageConnectionString` parameters.
    - Deleted properties `EventHubConnectionString` and `StorageConnectionString`.
    - Added constructor with `FullyQualifiedNamespace` and `BlobStorageServiceUri` parameters.
    - Require `Credential` in constructor; use the new property on `IntegrationTestConfiguration`.
    - Added properties `FullyQualifiedNamespace` and `BlobContainerUri`.

## Version 6.3.0

- Refactored class `IntegrationTestConfiguration`:
    - Deleted obsolete property `ApplicationInsightsInstrumentationKey`.
    - Marked property `ServiceBusConnectionString` as obsolete.
    - Added property `ServiceBusFullyQualifiedNamespace`.
- Refactored class `ServiceBusResourceProvider`:
    - Marked constructor with `ConnectionString` parameter as obsolete.
    - Marked property `ConnectionString` as obsolete.
    - Added constructor with `FullyQualifiedNamespace` parameter.
    - Added property `FullyQualifiedNamespace`.
- Refactored class `ServiceBusListenerMock`:
    - Marked constructor with `ConnectionString` parameter as obsolete.
    - Marked property `ConnectionString` as obsolete.
    - Added constructor with `FullyQualifiedNamespace` parameter.
    - Added property `FullyQualifiedNamespace`.

## Version 6.2.0

- Added parameter `UseSilentMode` with default value `true` to `AzuriteManager`. This disables access log output, which can be useful to reduce noise in test logs.

## Version 6.1.0

- Added `OpenIdJwtManager` to enable testing DH3 applications that requires OpenId and JWT for HTTP authentication and authorization, with the following features:
    - Starting an OpenId JWT server mock used for running tests that require OpenId configuration endpoints.
    - Creating internal JWT's used for testing DH3 applications that require authentication and authorization.
    - Creating fake JWT's used for testing that clients cannot authorize using incorrect tokens.
    - Rename & move certificate `/Azurite/TestCertificate/azurite-cert.pfx` to `/TestCertificate/test-common-cert.pfx` since it is now also used by the OpenId configuration server.

## Version 6.0.0

- Breaking changes
    <!-- markdown-link-check-disable-next-line -->
    - TestCommon <= 5.3.0 uses appregistrations which has been renamed in 6.0.0, see [dh3-infrastructure PR-1613](https://github.com/Energinet-DataHub/dh3-infrastructure/pull/1613/files) for details. Legacy app registrations referenced in versions <= 5.3.1 should be considered deprecated and product teams should upgrade to TestCommon 6.0.0 now.
    - Once legacy app registrations has been removed from B2C instance, test runs using TestCommon versions <= 5.3.1 will cause authenticationtests to fail.

## Version 5.3.0

- Added extensions to class `DatabricksSchemaManager`:
    - Method `InsertFromCsvFileAsync`

## Version 5.2.1

- Extended `IntegrationTestConfiguration` with the option to pass in `DefaultAzureCredential` from the outside.

## Version 5.2.0

- Changes to `SqlServerDatabaseManager`:
    - Extended constructor of `SqlServerDatabaseManager` with parameter `collationName` to support the creation of databases with other collation names than the `DefaultCollationName`.
    - Added const `DurableTaskCollationName` to allow easy access to the collation name necessary for the Durable Task SQL Provider.

## Version 5.1.2

- No functional change

## Version 5.1.1

- Updated Azurite test certificate.

## Version 5.1.0

- Added extensions to class `FunctionAppHostManagerExtensions`:
    - Method `CheckIfFunctionThrewException`
    - Method `AssertFunctionWasExecutedAsync`

## Version 5.0.1

- No functional change

## Version 5.0.0

- Deleted `FunctionApp.TestCommon.B2C` namespace with classes.
- Upgraded to .NET 8 SDK
- Refactored EventHub related classes to use the latest `Microsoft.Identity.Client` for token retrieval.

## Version 4.6.1

- No functional change

## Version 4.6.0

- Extended `EventHubResource` with possibility to add consumer groups

## Version 4.5.0

- Added property `ApplicationInsightsConnectionString` to `IntegrationTestConfiguration` to support use of connection string when using Application Insights.
- Marked property `ApplicationInsightsInstrumentationKey` on `IntegrationTestConfiguration` as obsolete.

## Version 4.4.3

- No functional change

## Version 4.4.2

- Databricks integration test configuration keys in keyvault are renamed

## Version 4.4.1

- No functional change

## Version 4.4.0

- Implemented `DatabricksSchemaManager` to support use of Databricks Sql statement Api

## Version 4.3.5

- No functional change

## Version 4.3.4

- No functional change

## Version 4.3.3

- Updated Azurite TestCertificate readme file

## Version 4.3.2

- Renewed Azurite certificate

## Version 4.3.1

- No functional change

## Version 4.3.0

- Extended class 'AzuriteManager' to support use of Queue and Table services.

## Version 4.2.0

- Added property `DatabricksSettings` to `IntegrationTestConfiguration` to support use of Databricks workspace and SQL Warehouse.

## Version 4.1.1

- No functional change.

## Version 4.1.0

- Extended class `AzuriteManager` to support use of OAuth.

## Version 4.0.3

- Add testresults to CI report

## Version 4.0.2

- No functional change.

## Version 4.0.1

- Bump version as part of pipeline change.

## Version 4.0.0

- Refactored class `B2CAuthorizationConfiguration` to use setting named `AZURE_B2CSECRETS_KEYVAULT_URL` instead of the variants `AZURE_SYSTEMTESTS_KEYVAULT_URL` and `AZURE_SECRETS_KEYVAULT_URL`.

## Version 3.5.2

- Bump version as part of pipeline change

## Version 3.5.1

- Bump version as part of pipeline change

## Version 3.5.0

- Added support for creating message type filters on Service bus subscriptions [Examples](../servicebusresourceprovider.md#examples)

## Version 3.4.0

- Added support for creating filters on Service bus subscriptions [Examples](../servicebusresourceprovider.md#examples)

## Version 3.3.0

- Implemented classes in `Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C` namespace, to support tests using B2C access tokens. See [Azure AD B2C token retrieval](../functionapp-testcommon.md#azure-ad-b2c-token-retrieval).

## Version 3.2.2

- Bump version as part of pipeline change

## Version 3.2.1

- Bump version as part of pipeline change

## Version 3.2.0

- Use default .NET Core SDK version pre-installed on Github Runner when running CI workflow

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
