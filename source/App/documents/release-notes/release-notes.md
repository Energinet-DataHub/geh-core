# App Release notes

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
- Implemented Function App extension `AddApplicationInsights` documented [here](..\extensions.md#application-insights).
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

- Implemented Health Check support for Azure Function App's and ASP.NET Core Web API's. See [Health Checks](../health-checks.md).

## Version 2.0.2

- Update `FunctionContextExtensions.Is` to compare trigger type name in a case insensitive way.

## Version 2.0.1

- The version was released, but no info was given here, so this is just to track the version.

## Version 2.0.0

- Update User to include multiple actor ids.

## Version 1.0.1

- Initial release
