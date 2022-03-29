# TraceContext Documentation

Contains a middleware implementation of TraceContext parsing to help with retrieving CorrelationId and ParentId from the TraceContext.

For more information on TraceContext please visit: https://www.w3.org/TR/trace-context/#trace-context-http-headers-format

## Usage

After the middleware have been called, you can get access to the id or parent trough `ICorrelationContext`

```c#
public class MyClass

private readonly ICorrelationContext _correlationContext;

public MyClass(ICorrelationContext correlationContext)
{
  _correlationContext = correlationContext;
}

public string CorrelationId()
{
 return _correlationContext.Id;
}

public string ParentId()
{
 return _correlationContext.ParentId;
}
```

## Registration

`CorrelationIdMiddleware` should be registered as a middleware and its lifetime should be `scoped`
`CorrelationContext` should be registered with lifetime`scoped`

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