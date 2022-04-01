# FunctionTelemetryScopeMiddleware Documentation

Contains a middleware implementation of TelemetryClient to help with handling the TelemetryDependency's. 

For more information on TelemetryClient please visit: https://docs.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.telemetryclient?view=azure-dotnet

## Usage

The middleware will handle the TelemetryDependency, which will enable a End-To-End Transaction overview within Application Insight.

## Registration

1. `FunctionTelemetryScopeMiddleware` should be registered as a middleware and its lifetime should be `scoped`.

```c#
protected virtual void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder options)
{
    options.UseMiddleware<FunctionTelemetryScopeMiddleware>();
}

private void ConfigureServices(IServiceCollection serviceCollection)
{    
    serviceCollection.AddScoped<FunctionTelemetryScopeMiddleware>();           
}
```