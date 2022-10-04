# App Release notes

## Version 7.2.0

- Add factory `AuthorizedHttpClientFactory` to create a `System.Net.Http.HttpClient`, which will re-apply the authorization header
  from the current HTTP context.

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