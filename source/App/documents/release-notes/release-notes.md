# App Release notes

## Version 15.0.0

- Upgrade from .NET 8 (8.0.100) to .NET 9 (9.0.100)

## Version 14.0.4

- Fix build error. Warning: (IDE0040)
- No functional change.

## Version 14.0.3

- Update tj-actions to v46.0.1
- No functional change.

## Version 14.0.2

- Update .github referencess to v14
- No functional change.

## Version 14.0.1

- Bump various NuGet packages to latest versions.
- No functional change.

## Version 14.0.0

- Bump various NuGet packages to latest versions.

## Version 13.3.2

- Bump various NuGet packages to latest versions.
- No functional change.

## Version 13.3.1

- Bump Energinet.DataHub.Core.TestCommon
- No functional change.

## Version 13.3.0

- Added support for optional SwaggerUI description in WebApi

## WebaApp, Common, Common.Abstractions, FunctionApp Version 13.2.0

- Added support for x-enumNames in OpenApiExtensions, which allows for the use of Enum names in the OpenAPI documentation.

## Version 13.1.0

- Extended health checks with an additional category `status`. Health checks can then be registerede as beeing called as part of the `ready` or the `status` endpoint.
    - In Azure Functions App the status endpoint is automatically available if the application already registerede health checks according to the documentation.
    - In ASP.NET Core Web API developers must add a call to `MapStatusHealthChecks()`. See the [Quick start for application startup](../documentation.md#quick-start-for-application-startup).

## Version 13.0.1

- In `FunctionApp` project:
    - Updated dependency to `DarkLoop.Azure.Functions.Authorization.Isolated` to fix [issue](https://github.com/dark-loop/functions-authorize/issues/62).

## Version 13.0.0

- See also [Version 13.0.0 release notes](./version_13_0_0.md)
- In `FunctionApp` project:
    - Refactored implementation to only work with [ASP.NET Core integration for HTTP](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#aspnet-core-integration).
    - Added functionality to configure authentication and authorization for HttpTrigger's
- In `WebApp` project:
    - Removed the obsolete overload of `AuthenticationExtensions.AddJwtBearerAuthenticationForWebApp`
- Moved type `Energinet.DataHub.Core.App.WebApp.Extensions.Options.UserAuthenticationOptions` to `Energinet.DataHub.Core.App.Common.Extensions.Options.UserAuthenticationOptions`.

## Version 12.2.1

- In `FunctionApp` project:
    - Replaced dependency `System.IdentityModel.Tokens.Jwt` with `Microsoft.IdentityModel.JsonWebTokens`

## Version 12.2.0

- For `WebApp` dependency injection, added call to `SwaggerGenOptions.UseAllOfToExtendReferenceSchemas` in `OpenApiExtensions.AddSwaggerForWebApp`.

## Version 12.1.0

- In `FunctionApp` project:
    - Add `Middleware.UserMiddleware`
    - Add reusable dependency injection extensions:
        - Add `AuthenticationBuilderExtensions.UseUserMiddlewareForIsolatedWorker<TUser>`
        - Add `AuthenticationExtensions.AddUserAuthenticationForIsolatedWorker<TUser, TUserProvider>`

## Version 12.0.1

- No functional change

## Version 12.0.0

- Add `Common` extensibility types:
    - `ApplicationInsights.SubsystemInitializer`
- Add `Common` reusable dependency injection extensions:
    - `HealthChecksExtensions.TryAddHealthChecks`
    - `NodaTimeExtensions.AddNodaTimeForApplication` with `DateTimeOptions`
- Add `FunctionApp` reusable dependency injection extensions:
    - `ApplicationInsightsExtensions.AddApplicationInsightsForIsolatedWorker`
    - `HealthChecksExtensions.AddHealthChecksForIsolatedWorker`
    - `LoggingBuilderExtensions.AddLoggingConfigurationForIsolatedWorker`
- Add `WebApp` reusable dependency injection extensions:
    - `ApiVersioningExtensions.AddApiVersioningForWebApp`
    - `ApplicationInsightsExtensions.AddApplicationInsightsForWebApp`
    - `HealthChecksExtensions.AddHealthChecksForWebApp`
    - `OpenApiExtensions.AddSwaggerForWebApp` with `ConfigureSwaggerOptions`
    - `OpenApiBuilderExtensions.UseSwaggerForWebApp`
- In `Common` project:
    - Moved builder extensions `HealthChecksBuilderExtensions` to namespace `Energinet.DataHub.Core.App.Common.Extensions.Builder`
- In `FunctionApp` project:
    - Removed extension `AddApplicationInsights` and the namespaces and types `FunctionTelemetryScope.*`
- In `WebApp` project:
    - Moved builder extensions `HealthCheckEndpointRouteBuilderExtensions` to namespace `Energinet.DataHub.Core.App.WebApp.Extensions.Builder`
    - Moved extensions `AuthenticationExtensions` to namespace `Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection` and refactored to follow new guidelines.
    - Moved extensions `AuthorizationExtensions` to namespace `Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection` and refactored to follow new guidelines.

## Version 11.1.0

- Prepared auth for MitID

## Version 11.0.0

- Upgraded project to .NET 8
- Upgraded dependencies to latest .NET 8

## Version 10.0.0

- Upgraded to .NET 7

## Version 9.0.0

- Removed code that was not used any where:
    - The `Common.Security` project.
    - In `Common` project the namespaces and types `Identity.*`, `Parsers.*`, `ActorContext`.
    - In `Common.Abstractions` project the namespaces and types `Actors.*`, `Identity.*`, `IntegrationEventContext.*`, `Security.*` and `ServieBus.*`.
    - In `FunctionApp` project the namespaces and types `Middleware.*`.
    - In `WebApp` project the namespaces and types `Hosting.*`.

## Version 8.3.2

- No functional change

## Version 8.3.1

- Removed custom property "HostedService" and structured logging placeholder "Worker" from `RepeatingTrigger` because the same information is available in the property `CategoryName`.

## Version 8.3.0

- Updated package version for `Energinet.DataHub.Core.JsonSerialization`.

## Version 8.2.1

- No functional change

## Version 8.2.0

- Rename parameter isFas to multiTenancy in IUserProvider.

## Version 8.1.1

- No functional change.

## Version 8.1.0

- Support the retrieval of DH3 source version information from an .NET assembly.
- Refactored Live health check to return DH3 source version information when available.

## Version 8.0.0

- Deleted packages `Energinet.DataHub.Core.App.FunctionApp.SimpleInjector` and `Energinet.DataHub.Core.App.WebApp.SimpleInjector`.

## Version 7.6.0

- Changed Health Check response format for Azure Function App's and ASP.NET Core Web API's to support the use of Health Checks UI. See [Health Checks](../registrations/health-checks.md).

## Version 7.5.3

- No functional change.

## Version 7.5.2

- No functional change.

## Version 7.5.1

Bug fixing `RepeatingTrigger<TService>` including related health checks.

## Version 7.5.0

Add app hosting functionality:

- Add hosted service `RepeatingTrigger<TService>` that can be used to run a hosted service at a fixed interval.
  The trigger will wait for a certain amount of time since last invocation terminated before starting a new invocation.
- Health checks of `RepeatingTrigger<TService>` can be added using

```csharp
services.AddHealthChecks().AddRepeatingTriggerHealthCheck<MyRepeatingTrigger>(timeoutTimeSpan);
```

## Version 7.4.7

- No functional change.

## Version 7.4.6

- No functional change.

## Version 7.4.5

- No functional change.

## Version 7.4.4

- No functional change.

## Version 7.4.3

- No functional change.

## Version 7.4.2

- Add testresults to CI report

## Version 7.4.1

- No functional change.

## Version 7.4.0

- Permissions moved to geh-market-participant.

## Version 7.3.4

- Bump version as part of pipeline change.

## Version 7.3.3

- Added new permission UserRoleManage

## Version 7.3.2

- Bump version as part of pipeline change

## Version 7.3.1

- Bump version as part of pipeline change

## Version 7.3.0

- Updated packages.
- Removed previous code for user JWT middleware.

## Version 7.2.11

- Added new permission UsersView.

## Version 7.2.10

- Added new permission UsersManage.

## Version 7.2.9

- Skip unknown endpoints in UserMiddleware.

## Version 7.2.8

- Detect whether the user is FAS member.

## Version 7.2.7

- Added new permission ActorManage.

## Version 7.2.6

- Renamed missed Actor namespace to Actors, updated

## Version 7.2.5

- Changed Actor namespace to Actors, updated references
- Added values to Permission Enums

## Version 7.2.4

- Support for DataHub's own tokens.

## Version 7.2.3

- Fixed wrong claim names when reading token.

## Version 7.2.2

- Added SimpleInjector extension for user middleware.

## Version 7.2.1

- Add support for getting user from token in web apps.
- Updated authorization documentation.

## Version 7.2.0

- Add support for authorization in web apps.

## Version 7.1.1

- Bump version as part of pipeline change

## Version 7.1.0

- Add extension method for calling health endpoints of other services.

## Version 7.0.0

- Change `JwtTokenValidator` constructor. Dependent types should be configured for dependency injection. The purpose of these changes is to reuse (cache) Open ID configuration used for JWT validation to avoid requesting these information for each http request validated.
- Deleted class `OpenIdSettings`.
- Updated `AddJwtTokenSecurity` extensions to match new dependency injection requirements.

## Version 6.0.0

- Change `CorrelationIdMiddleware` to use `CorrelationId` header instead of `Correlation-ID` header.

## Version 5.0.2

- Bump version as part of pipeline change

## Version 5.0.1

- ReadMetadata() now correctly throws when data is missing.

## Version 5.0.0

- **Beaking change:** Renamed class `TraceContext` to `TraceParent`.
- Implemented Function App extension `AddApplicationInsights`.
- Updated all dependent NuGet packags to latest versions.

## Version 4.1.1

- Bump version as part of pipeline change

## Version 4.1.0

- Use default .NET Core SDK version pre-installed on Github Runner when running CI workflow

## Version 4.0.2

- docs were added for IntegrationEventMetadataMiddleware

## Version 4.0.1

- Bump version as part of pipeline change

## Version 4.0.0

- Update to the namespace for the `FunctionTelemetryScope` middleware.
- CorrelationIdMiddleware now exposes `CorrelationId` from either a `HTTP-header` or a `ServicebusMessage` user property

## Version 3.0.1

- Bumped patch version as pipeline file was updated.

## Version 3.0.0

- Upgrade projects from .NET5 to .NET6

## Version 2.4.0

- Added IntegrationEventContext that provides access to integration event metadata using IntegrationEventMetadataMiddleware.

## Version 2.3.4

- Bumped patch version as pipeline file was updated.

## Version 2.3.3

- Introduced ability to create ActorMiddleware with exclusion of specific functions. This is commonly used when implementing HealthCheck in functions.

## Version 2.3.2

- Introduced ability to create JwtTokenMiddleware with exclusion of specific functions. This is commonly used when implementing HealthCheck in functions.

## Version 2.3.1

- Bumped patch version as pipeline file was updated.

## Version 2.3.0

- The version was released, but no info was given here, so this is just to track the version.

## Version 2.2.0

- CorrelationIdMiddleware was introduced for FunctionApp.

## Version 2.1.0

- Implemented Health Check support for Azure Function App's and ASP.NET Core Web API's. See [Health Checks](../registrations/health-checks.md).

## Version 2.0.2

- Update `FunctionContextExtensions.Is` to compare trigger type name in a case insensitive way.

## Version 2.0.1

- The version was released, but no info was given here, so this is just to track the version.

## Version 2.0.0

- Update User to include multiple actor ids.

## Version 1.0.1

- Initial release
