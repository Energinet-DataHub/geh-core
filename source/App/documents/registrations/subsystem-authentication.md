# Subsystem Authentication

By Subsystem Authentication we mean the server side authentication performed in subsystem-to-subssytem communication. Read ADR-141 in Confluence for more information.

## Overview

- Implementation
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

## Azure Functions App

Azure Functions apps must use [ASP.NET Core integration for HTTP](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#aspnet-core-integration). This allows us to use the ASP.NET Core types for supporting authentication and authorization for HttpTrigger's.

Endpoint authorization for HttpTrigger's is enforced by using the `Authorize` attribute. If the `AllowAnonymous` atttribute (or no attribute) is specified, the endpoint is not protected and allow anonymous access.

### Configuration of Authentication

- Add `UseFunctionsAuthorization()` to `IFunctionsWorkerApplicationBuilder`.
    - This registers services and middleware which allows us to use certain ASP.NET Core types, including the previously mentioned attributes.
- Add `AddSubsystemAuthenticationForIsolatedWorker()` to `IServiceProvider`.
    - This will enable verification of, and authentication by JWT.
- Configure application settings as specified by `SubsystemAuthenticationOptions`.

## ASP.NET Core Web API

> This is not supported yet in the App package. We will implement this in another PR.
