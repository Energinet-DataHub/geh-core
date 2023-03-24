# JWT Security

> Each domain must implement its own domain-specific actor authorization! The middleware ensures only that the token is valid. Failure to do so may lead to escalation of privileges.

- DO validate the signature, lifetime, audience and issuer of the token.
- STRONGLY RECOMMENDED to validate the external token from 'token'-claim. Middleware will do this for you.
- DO validate the actor id in the token (for example in `IUserProvider.ProvideUserAsync`).
- DO return `null` from `IUserProvider.ProvideUserAsync` as much as possible, e.g. if the actor id is unknown or irrelevant.
- DO trust the actor id only from `IUserContext`.
- DO treat actors with same security considerations as if they were separate tenants.
- DO create a `TUser` implementation that is convenient for your domain.

## Authorization in Web Apps

Endpoint authorization in web apps is enforced by role-based authorization (see <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles>).
Every supported permission is configured as a role claim, using the built-in framework to ensure that the user is both authenticated and has this claim.
Should authorization fail, the endpoint will return 403 Forbidden.

### Configuration of Authentication

Before enabling authorization, the authentication must be configured first. This ensures that the token is signed, obtained from the correct tenant and that its authorized party is the frontend application.

- Add `UseAuthentication()` to `IApplicationBuilder`.
    - This will register the built-in authentication middleware.
    - See <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
- Add `AddJwtBearerAuthentication()` to `IServiceProvider`.
    - This will enable verification of and authentication by JWT, configuring the `ClaimsPrincipal`.

### Configuration of Authorization

Configuring authorization is very similar.

- Add `UseAuthorization()` after `UseAuthentication()` to `IApplicationBuilder`.
    - See <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
- Add `AddPermissionAuthorization()` to `IServiceProvider`.
    - This will register the permissions with the framework.

### Configuration of IUserProvider

Configuring middleware for obtaining the current user with the current actor.

- Implement `TUserProvider` and `TUser`.
- Add `UseUserMiddleware<TUser>()` after `UseAuthorization()` to `IApplicationBuilder`.
    - This enables `UserMiddleware`.
- Add `AddUserAuthentication<TUser, TUserProvider>()` to `IServiceProvider()`.
    - This registers `UserMiddleware`, `IUserProvider` and `IUserContext`.

### Example Configuration

`DomainUser` is a domain-specific implementation of a user. `DomainUserProvider` is a domain-specific implementation of `IUserProvider<TUser>`.

```C#
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseUserMiddleware<DomainUser>();

    var externalOpenIdUrl = ...;
    var internalOpenIdUrl = ...;
    var backendAppId = ...;
    services.AddJwtBearerAuthentication(externalOpenIdUrl, internalOpenIdUrl, backendAppId);
    services.AddUserAuthentication<DomainUser, DomainUserProvider>();
    services.AddPermissionAuthorization();
```

### Usage

This package includes an `AuthorizeUserAttribute` for selecting a supported permission.
The attribute can be used to annotate Controller classes or individual methods within.

For example, if an endpoint requires 'Organization' permission, the attribute can be used as follows.

```C#
    [HttpGet]
    [Authorize(Roles = "organizations:view")]
    public async Task<IActionResult> GetExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (Organization || GridAreas), if an endpoint requires any of the specified permissions.

```C#
    [HttpPost]
    [Authorize(Roles = "organizations:view, grid-areas:manage")]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (Organization && GridAreas), if an endpoint requires both of the specified permissions.

```C#
    [HttpPut]
    [Authorize(Roles = "organizations:view")]
    [Authorize(Roles = "grid-areas:manage")]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```
