# Schema Validation Documentation

A schema validating forward-only reader for XML (and eventually JSON) documents.

## Usage
Create an instance of the reader, passing in the data stream and the schema. Then read from the stream using the API provided by the reader. When `AdvanceAsync` returns `false`, check whether any errors have occurred during reading.

```c#
var schemaValidatingReader = new SchemaValidatingReader(
    stream,
    Schemas.CimXml.StructureGenericNotification)

while (await schemaValidatingReader.AdvanceAsync().ConfigureAwait(false))
{
    ...
}

if (schemaValidatingReader.HasErrors)
{
    ...
}
```

## Error Response
In case of validation errors, a `CreateErrorResponse` extension method has been provided. This method will convert the validation errors into a format suitable for returning externally. Use `WriteAsXmlAsync` to write the error response as XML into the specified stream.

```c#
if (schemaValidatingReader.HasErrors)
{
    var responseStream = ...;
    await schemaValidatingReader
        .CreateErrorResponse()
        .WriteAsXmlAsync(responseStream)
        .ConfigureAwait(false);
}
```

- A `WriteAsJsonAsync` will be provided in the future.