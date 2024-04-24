# Logging Middleware for Request and Response Release notes

## Version 3.1.5

- Bump System.IdentityModel.Tokens.Jwt from 6.15.0 to 6.34.0 because of CVE-2024-21319

## Version 3.1.4

- No functional change.

## Version 3.1.3

- No functional change.

## Version 3.1.2

- No functional change.

## Version 3.1.1

- No functional change.

## Version 3.1.0

- Added `SetApplicationInsightLogLevel` for function apps.

## Version 3.0.0

Removed content of package `Energinet.DataHub.Core.Logging`.
The content was unused and a lot of it did not fit a `Logging` package.

Moved `LoggingScope` and `RootLoggingScope` to package `Energinet.DataHub.Core.Logging`.

## Version 2.3.1

- No functional change.

## Version 2.3.0

- Added new LoggingScopeMiddleware package
    - Added HttpLoggingScopeMiddleware
    - Added FunctionLoggingScopeMiddleware

## Version 2.2.8

- No functional change.

## Version 2.2.7

- No functional change.

## Version 2.2.6

- No functional change.

## Version 2.2.5

- Add testresults to CI report

## Version 2.2.4

- No functional change.

## Version 2.2.3

- Bump version as part of pipeline change.

## Version 2.2.2

- Bumped patch version as pipeline file was updated.

## Version 2.2.1

- Bumped patch version as pipeline file was updated.

## Version 2.2.0

- Use default .NET Core SDK version pre-installed on Github Runner when running CI workflow

## Version 2.1.0

- Upgrade Azure.Storage.Blobs to 12.13.0
- Changed the way query parameters are parsed.
- BUG FIX: Json in request body now does not throw error.

## Version 2.0.0

- Upgrade from .NET 5 to .NET 6

## Version 1.2.3

- Bumped patch version as pipeline file was updated.

## Version 1.2.2

- Bumped patch version as pipeline file was updated.

## Version 1.2.1

- Fixing log file naming

## Version 1.2.0

- Changes the way tags are selected and saved with the log.
- Requests with /monitor/ in Url are skipped because of HealthCheck.

## Version 1.0.7

- Better file naming for cleaner blob urls
- Added actorid jwt token parsing
- Removed gln jwt token parsing

## Version 1.0.6

- Added TraceId to metadata and index tags for better lookup with application insight logs.
- Limited query params saved in index tags to 3.

## Version 1.0.5

- Added logging condition to only run on httpTriggers.
- Added timers for debugging execution times.

## Version 1.0.4

- Updated jwt token parsing.

## Version 1.0.3

- Better naming with subfolder.
- Added jwt token parsing for index tags.

## Version 1.0.2

- Changes for log naming and request response stream handling

## Version 1.0.0

- Initial release.
