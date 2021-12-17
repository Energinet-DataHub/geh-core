# Schema Validation Documentation

A schema validating forward-only reader for XML (and eventually JSON) documents.

## Usage
Create an instance of the reader, passing in the data stream and the schema. Then read from the stream using the API provided by the reader. When `AdvanceAsync` returns `false`, check whether any errors have occurred during reading.

```c#
var schamaValidationReader = new SchemaValidationReader(
    stream,
    Schemas.Schemas.CimXml.StructureGenericNotification)

while (await schamaValidationReader.AdvanceAsync().ConfigureAwait(false))
{
    ...
}

if (schemaValidationReader.HasErrors)
{
    ...
}
```

## Error Response
In case of validation errors, a `CreateErrorResponse` extension method has been provided. This method will convert the validation errors into a format suitable for returning externally. Use `WriteAsXmlAsync` to write the error response as XML into the specified stream.

```c#
if (schemaValidationReader.HasErrors)
{
    var responseStream = ...;
    await schamaValidationReader
        .CreateErrorResponse()
        .WriteAsXmlAsync(responseStream)
        .ConfigureAwait(false);
}
```

- A `WriteAsJsonAsync` will be provided in the future.