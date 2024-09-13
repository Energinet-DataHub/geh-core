# Version 13.0.0

## Azure Functions App

Breaking change: Consumers of the `FunctionApp` package must use [ASP.NET Core integration for HTTP](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#aspnet-core-integration).

When using ASP.NET Core integration for HTTP it is still possible to have HttpTrigger's that uses the `HttpRequestData` type, which is why we are not forced to rewrite the `HealthCheckEndpoint` class in existing Azure Functions applications.

Middleware that uses the `HttpRequestData` should however be refactored to use `HttpContext` and other ASP.NET Core types.

For detailed information of how to use the new capabilities, see [JWT Security](../registrations/authorization.md).

### How to upgrade

1) In `Program.cs` update the call to `ConfigureFunctionsWorkerDefaults` with a call to `ConfigureFunctionsWebApplication`.

2) If the extension `UseUserMiddlewareForIsolatedWorker` is used and the application has HttpTrigger's that is not part of the `HealthCheckEndpoint` class:
    - Either rewrite these HttpTrigger's to use the ASP.NET Core types, or exclude these functions so they are not handled by the middleware. To exclude functions use the parameter `excludedFunctionNames`.

3) Any existing middleware that uses `HttpRequestData` should be refactored to use `HttpContext` instead, and the HttpTrigger's for which the middleware is used should be refactored to use ASP.NET Core types.

## ASP.NET Core Web API

There is no breaking change for consumers of the `WebApp` package.
