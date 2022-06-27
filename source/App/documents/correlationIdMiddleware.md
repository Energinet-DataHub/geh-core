# CorrelationContext Documentation

Contains a middleware implementation of exposing the `CorrelationId` from the `FunctionContext` from either a `HttpTrigger` or `ServiceBusTrigger`.

### HttpTrigger
The CorrelationId is parsed from a http-header named `Correlation-ID`

### ServiceBusTrigger
The CorrelationId is parsed from a user property named `OperationCorrelationId`

## Usage

After the middleware have been called, you can get access to the id or parent through `ICorrelationContext`

```c#
public class MyClass
{
    private readonly ICorrelationContext _correlationContext;

    public MyClass(ICorrelationContext correlationContext)
    {
        _correlationContext = correlationContext;
    }

    public string CorrelationId()
    {
        return _correlationContext.Id;
    }
}


```

## Registration

1. `CorrelationIdMiddleware` should be registered as a middleware and its lifetime should be `scoped`.
1. `CorrelationContext` should be registered with lifetime`scoped`.

```c#
protected virtual void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
{
    options.UseMiddleware<CorrelationIdMiddleware>();
}

private void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddScoped<ICorrelationContext, CorrelationContext>();
    serviceCollection.AddScoped<CorrelationIdMiddleware>();           
}
```