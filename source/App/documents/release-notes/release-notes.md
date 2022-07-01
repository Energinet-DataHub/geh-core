# App Release notes

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