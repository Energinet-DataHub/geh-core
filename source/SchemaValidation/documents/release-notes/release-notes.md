# Schema Validation Release notes

## Version 2.1.8

- No functional change.

## Version 2.1.7

- No functional change.

## Version 2.1.6

- No functional change.

## Version 2.1.5

- Add testresults to CI report

## Version 2.1.4

- No functional change.

## Version 2.1.3

- Bump version as part of pipeline change.

## Version 2.1.2

- Bumped patch version as pipeline file was updated.

## Version 2.1.1

- Bumped patch version as pipeline file was updated.

## Version 2.1.0

- Use default .NET Core SDK version pre-installed on Github Runner when running CI workflow

## Version 2.0.0

- Upgrade .NET 5 to .NET 6

## Version 1.0.13

- Bumped patch version as pipeline file was updated.

## Version 1.0.12

- Bumped patch version as pipeline file was updated.

## Version 1.0.11

- Changed the `code` returned on schema validation error from `BadArgument` to `B2B-005`

## Version 1.0.10

- Bumped patch version as pipeline file was updated.

## Version 1.0.9

Add support for custom namespace prefixes

## Version 1.0.8

Rename source folder and solution name for easier developer discovery in GitHub repo.

## Version 1.0.7

- AdvanceAsync() now reads content to ensure it is properly validated.
- Workaround of a bug in AdvanceAsync(), where XmlReader could throw an InvalidOperationException for invalid XML.
   This only applies to the Reader API - extension methods AsXmlReaderAsync and AsXElementAsync are not affected.

## Version 1.0.6

- Added ReadValueAsDurationAsync() for xs:duration data types.

## Version 1.0.5

- CanReadValue could incorrectly return false when an attribute was preceding the element content.
   This only applies to the Reader API - extension methods AsXmlReaderAsync and AsXElementAsync are not affected.

## Version 1.0.4

- No API changes.
- Schemas moved to separate solution.

## Version 1.0.3

- Added CreateErrorResponse extension method.
- Added WriteAsXmlAsync extension method.
- Added AsXmlReaderAsync extension method.

## Version 1.0.2

- Added AsXElementAsync extension method.

## Version 1.0.1

- Initial release.
