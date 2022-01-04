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

A concrete implementation of `XmlMappingConfigurationBase` must be added for each type that implements `IInternalMarketDocument`.

```c#
public class MasterDataDocumentXmlMappingConfiguration : XmlMappingConfigurationBase
{
    public MasterDataDocumentXmlMappingConfiguration()
    {
        CreateMapping<MasterDataDocument>("MktActivityRecord", mapper => mapper
            .AddProperty(x => x.PropertyName, "MarketEvaluationPoint", "xmlNodeName")
            .AddProperty(x => x.SpecialProperty, OptionalTranslationMethod, "MarketEvaluationPoint", "anotherXmlNodeName")
            ....
    }
    
    private static string OptionalTranslationMethod(XmlElementInfo element)
    {
        return element.SourceValue.ToUpperInvariant() switch
        {
            "E01" => "RealValue1",
            "E02" => "RealValue2",
            "E03" => "RealValue3",
            _ => element.SourceValue,
        };
    }
} 
```

Use XML deserializer as below

```c#
var xmlDeserializationResult = await _xmlConverter.DeserializeAsync(request.Body);
```