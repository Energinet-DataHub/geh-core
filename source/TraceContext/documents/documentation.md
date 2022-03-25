# TraceContext Documentation

Contains a middelware implmentation of TraceContext parsing to help with retrieving CorrelationId and ParentId from the TraceContext. 

## Usage
TBA

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