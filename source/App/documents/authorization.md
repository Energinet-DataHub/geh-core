# Authorization Documentation

The authorization is based on OAuth claims granted by an authorization server. The granted claims are placed in a JWT access token within the "roles"-claim. Each claim value represents and grants access to a single permission in DataHub.

As an example, the payload of an access token giving permissions `Organization` and `GridAreas` will look as follows.

```Json
{
  "sub": "<user-id>",
  "azp": "<frontend-app-id>",
  "aud": "<external-actor-id>",
  "roles": ["organization:view", "gridareas:manage"]
}
```

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

- Add `UseAuthorization()` after `UseUserAuthentication()` to `IApplicationBuilder`.
  - See <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
- Add `AddPermissionAuthorization()` to `IServiceProvider`.
  - This will register the permissions with the framework.

### Configuration of IUserProvider

Configuring middleware for obtaining the current user with the current actor.

- Add `UseUserAuthentication<TUser>()` after `UseAuthentication()` to `IApplicationBuilder`.
  - This enables `UserMiddleware`.
- Add `AddUserAuthentication<TUser, TUserProvider>()` to `IServiceProvider()`.
  - This registers `UserMiddleware`, `IUserProvider` and `IUserContext`.
- Implement `TUserProvider` and `TUser`.
  - `TUserProvider.ProvideUserAsync()` can return a domain-specific `TUser`. This user can later be obtained by dependency injection of `IUserContext<TUser>`.
  - `TUserProvider.ProvideUserAsync()` **should** return `null` as much as possible, e.g. if the external actor id was not recognized. This provides additional protection in case of misconfiguration.

### Example Configuration

`DomainUser` is a domain-specific implementation of a user. `DomainUserProvider` is a domain-specific implementation of `IUserProvider<TUser>`.

```C#
    app.UseAuthentication();
    app.UseUserAuthentication<DomainUser>();
    app.UseAuthorization();

    var openIdUrl = ...;
    var frontendAppId = ...;
    services.AddJwtBearerAuthentication(openIdUrl, frontendAppId);
    services.AddUserAuthentication<DomainUser, DomainUserProvider>();
    services.AddPermissionAuthorization();
```

### Usage

This package includes an `AuthorizeAttribute` for selecting a supported permission.
The attribute can be used to annotate Controller classes or individual methods within.

For example, if an endpoint requires 'Organization' permission, the attribute can be used as follows.

```C#
    [HttpGet]
    [Authorize(Permission.Organization)]
    public async Task<IActionResult> GetExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (Organization || GridAreas), if an endpoint requires any of the specified permissions.

```C#
    [HttpPost]
    [Authorize(Permission.Organization, Permission.GridAreas)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (Organization && GridAreas), if an endpoint requires both of the specified permissions.

```C#
    [HttpPut]
    [Authorize(Permission.Organization)]
    [Authorize(Permission.GridAreas)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```
