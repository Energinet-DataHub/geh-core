# Authorization Documentation

> NOTE: The 'extension_roles' claim is used instead of the proper 'roles'-claim, since App Roles are not supported by B2C Tenant. A side-effect of this is that the claim value is received as a string and not a JSON array.

The authorization is based on OAuth claims granted by an authorization server.
The granted claims are placed in a JWT access token within the "extension_roles"-claim.
Each claim value represents and grants access to a single role in DataHub.

As an example, the payload of an access token giving roles `Supporter` and `Accountant` will look as follows.

```Json
{
  "sub": "1234567890",
  "extension_roles": "["Supporter", "Accountant"]"
}
```

## Authorization in Web Apps

Endpoint authorization in web apps is enforced by role-based authorization (see <https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles>).
Every supported role is configured as a claim, using the built-in framework to ensure that the user is both authenticated and has this claim.
Should authorization fail, the endpoint will return 403 Forbidden.

### Configuration

Before enabling authorization, the authentication must be configured first.

1) Add `UseAuthentication()` to `IApplicationBuilder`, see <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>. This will register the authentication middleware.
2) Add `AddJwtBearerAuthentication()` to `IServiceProvider`. This will enable verification of and authentication by JWT, configuring the `ClaimsPrincipal`.

```C#
    var openIdUrl = ...;
    var audience = ...;
    services.AddJwtBearerAuthentication(openIdUrl, audience);
```

> NOTE: Because 'extension_roles' is received as a string, add `UseMiddleware<ExtensionRolesClaimMiddleware>()` after `UseAuthentication()`.

Configuring authorization is very similar.

1) Add `UseAuthorization()` after `UseAuthentication()` to `IApplicationBuilder`, see <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
2) Add `AddPermissionAuthorization()` to `IServiceProvider`. This will register the permissions with the framework.

### Usage

This package includes an `AuthorizeAttribute` for selecting a supported permission.
The attribute can be used to annotate Controller classes or individual methods within.

For example, if an endpoint requires 'Accountant' role, the attribute can be used as follows.

```C#
    [HttpGet]
    [Authorize(UserRoles.Accountant)]
    public async Task<IActionResult> GetExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple roles (Accountant || Supporter), if an endpoint requires any of the specified roles.

```C#
    [HttpPost]
    [Authorize(UserRoles.Accountant, UserRoles.Supporter)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple roles (Accountant && Supporter), if an endpoint requires both of the specified roles.

```C#
    [HttpPost]
    [Authorize(UserRoles.Accountant)]
    [Authorize(UserRoles.Supporter)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```
