# Schema Validation Release notes

## Version 1.0.7

 - AdvanceAsync() now ensures that the content has the correct type.
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
