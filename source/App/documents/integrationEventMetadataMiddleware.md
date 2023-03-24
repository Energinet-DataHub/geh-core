# IntegrationEventMetaData Documentation

Contains a middleware implementation exposing userproperties in accordance with "ADR-008 Message metadata".

## Supported properties

- OperationTimestamp
- OperationCorrelationId
- MessageType

## Usage

After the middleware have been called, you can get access to the metadata through `IIntegrationEventContext`

```c#
public class MyClass
{
    private readonly IIntegrationEventContext _integrationEventContext;

    public MyClass(IIntegrationEventContext integrationEventContext)
    {
        _integrationEventContext = integrationEventContext;
    }

    public string MessageType()
    {
        return _integrationEventContext.MessageType;
    }
}

```

## Registration

1. `IntegrationEventMetadataMiddleware` should be registered as a middleware and its lifetime should be `scoped`.
2. `IIntegrationEventContext` should be registered with lifetime`scoped`.

```c#
protected virtual void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
{
    options.UseMiddleware<IntegrationEventMetadataMiddleware>();
}

private void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddScoped<IJsonSerializer, JsonSerializer>();
    serviceCollection.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
    serviceCollection.AddScoped<IntegrationEventMetadataMiddleware>();
}
```
