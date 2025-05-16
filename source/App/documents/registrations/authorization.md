# User Authentication and Authorization

> Each subsystem must implement its own subsystem-specific actor authorization! The middleware ensures only that the token is valid. Failure to do so may lead to escalation of privileges.

- DO validate the signature, lifetime, audience and issuer of the token.
- STRONGLY RECOMMENDED to validate the external token from 'token'-claim. Middleware will do this for you.
- DO validate the actor id in the token (for example in `IUserProvider.ProvideUserAsync`).
- DO return `null` from `IUserProvider.ProvideUserAsync` as much as possible, e.g. if the actor id is unknown or irrelevant.
- DO trust the actor id only from `IUserContext`.
- DO treat actors with same security considerations as if they were separate tenants.
- DO create a `TUser` implementation that is convenient for your subsystem.

## Overview

- Implementation
    - [Azure Functions App](#azure-functions-app)
    - [ASP.NET Core Web API](#aspnet-core-web-api)

## Azure Functions App

Azure Functions apps must use [ASP.NET Core integration for HTTP](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#aspnet-core-integration). This allows us to use the ASP.NET Core types for supporting authentication and authorization for HttpTrigger's.

Endpoint authorization for HttpTrigger's is enforced by role-based authorization using the `Authorize` attribute, similar to what is shown below for ASP.NET Core Web API under [Usage](#usage). The `AllowAnonymous` attribute is also supported.

### Configuration of Authentication and Authorization

- Add `UseFunctionsAuthorization()` to `IFunctionsWorkerApplicationBuilder`.
    - This registers services and middleware which allows us to use certain ASP.NET Core types, including the previously mentioned attributes.
- Add `AddJwtBearerAuthenticationForIsolatedWorker()` to `IServiceProvider`.
    - This will enable verification of and authentication by JWT, configuring the `ClaimsPrincipal`.

### Configuration of IUserProvider

Configuring middleware for obtaining the current user with the current actor. This middleware depends on services registered by `UseFunctionsAuthorization()`.

- Implement `TUserProvider` and `TUser`.
- Add `UseUserMiddlewareForIsolatedWorker<TUser>()` to `IFunctionsWorkerApplicationBuilder`.
    - This registers and enables `UserMiddleware`.
- Add `AddUserAuthenticationForIsolatedWorker<TUser, TUserProvider>()` to `IServiceProvider()`.
    - This registers `IUserProvider` and `IUserContext`.

## ASP.NET Core Web API

Endpoint authorization in web apps is enforced by role-based authorization (see <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles>).
Every supported permission is configured as a role claim, using the built-in framework to ensure that the user is both authenticated and has this claim.
Should authorization fail, the endpoint will return 403 Forbidden.

### Configuration of Authentication

Before enabling authorization, the authentication must be configured first. This ensures that the token is signed, obtained from the correct tenant and that its authorized party is the frontend application.

- Add `UseAuthentication()` to `IApplicationBuilder`.
    - This will register the built-in authentication middleware.
    - See <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
- Add `AddJwtBearerAuthenticationForWebApp()` to `IServiceProvider`.
    - This will enable verification of and authentication by JWT, configuring the `ClaimsPrincipal`.

### Configuration of Authorization

Configuring authorization is very similar.

- Add `UseAuthorization()` after `UseAuthentication()` to `IApplicationBuilder`.
    - See <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
- Add `AddPermissionAuthorizationForWebApp()` to `IServiceProvider`.
    - This will register the permissions with the framework.

### Configuration of IUserProvider

Configuring middleware for obtaining the current user with the current actor.

- Implement `TUserProvider` and `TUser`.
- Add `UseUserMiddlewareForWebApp<TUser>()` after `UseAuthorization()` to `IApplicationBuilder`.
    - This enables `UserMiddleware`.
- Add `AddUserAuthenticationForWebApp<TUser, TUserProvider>()` to `IServiceProvider()`.
    - This registers `UserMiddleware`, `IUserProvider` and `IUserContext`.

### Example Configuration

`SubsystemUser` is a subsystem-specific implementation of a user. `SubsystemUserProvider` is a subsystem-specific implementation of `IUserProvider<TUser>`.

```C#
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseUserMiddlewareForWebApp<SubsystemUser>();

    // Settings are loaded into 'UserAuthenticationOptions' from configuration
    services.AddJwtBearerAuthenticationForWebApp(configuration);
    services.AddUserAuthenticationForWebApp<SubsystemUser, SubsystemUserProvider>();
    services.AddPermissionAuthorizationForWebApp();
```

### Usage

The built-in `AuthorizeAttribute` attribute can be used to annotate Controller classes or individual methods within. For example, if an endpoint requires 'organization:view' permission, the attribute can be used as follows.

```C#
    [HttpGet]
    [Authorize(Roles = "organizations:view")]
    public async Task<IActionResult> GetExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (organizations:view || grid-areas:manage), if an endpoint requires any of the specified permissions.

```C#
    [HttpPost]
    [Authorize(Roles = "organizations:view, grid-areas:manage")]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (organizations:view && grid-areas:manage), if an endpoint requires both of the specified permissions.

```C#
    [HttpPut]
    [Authorize(Roles = "organizations:view")]
    [Authorize(Roles = "grid-areas:manage")]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```
