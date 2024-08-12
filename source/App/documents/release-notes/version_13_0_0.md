# Version 13.0.0

## Azure Functions App

Breaking change: Consumers of the `FunctionApp` package must use [ASP.NET Core integration for HTTP](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#aspnet-core-integration).

When using ASP.NET Core integration for HTTP it is still possible to have HttpTrigger's that uses the `HttpRequestData` type, which is why we are not forced to rewrite the `HealthCheckEndpoint` class in existing Azure Functions applications.

Middleware that uses the `HttpRequestData` should however be refactored to use `HttpContext` and other ASP.NET Core types.

For detailed information of how to use the new capabilities, see [JWT Security](../registrations/authorization.md).

### How to upgrade

1) In `Program.cs` update the call to `ConfigureFunctionsWorkerDefaults` with a call to `ConfigureFunctionsWebApplication`.

## ASP.NET Core Web API

There is no breaking change for consumers of the `WebApp` package.
