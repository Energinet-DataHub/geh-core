# XML Converter Documentation

### Introduction

A library containing functionality for conversion of CIM XML to C#.

### Usage

Install following packages

* `Energinet.DataHub.Core.XmlConversion.XmlConverter`
* `Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions`
  
Register in IoC (in example below SimpleInjector is used)

```c#
protected override void ConfigureContainer(Container container)
{
    container.Register(() => new XmlMapper(XmlMappingConfiguration, TranslateProcessType), Lifestyle.Singleton);
    container.Register<IXmlConverter, XmlDeserializer>(Lifestyle.Singleton);
    ...
}

private static XmlMappingConfigurationBase XmlMappingConfiguration(string documentType)
{
    return documentType.ToUpperInvariant() switch
    {
        "E58" => new MasterDataDocumentXmlMappingConfiguration(),
        _ => throw new NotImplementedException(documentType),
    };
}

private static string TranslateProcessType(string processType)
{
    return processType.ToUpperInvariant() switch
    {
        "E02" => nameof(BusinessProcessType.CreateMeteringPoint),
        "D15" => nameof(BusinessProcessType.ConnectMeteringPoint),
        "E32" => nameof(BusinessProcessType.ChangeMasterData),
        _ => throw new NotImplementedException(processType),
    };
}
```

Use XML deserializer as below 

```c#
await _xmlConverter.DeserializeAsync(request.Body).ConfigureAwait(false);
```