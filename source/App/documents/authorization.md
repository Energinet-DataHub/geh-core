# Authorization Documentation

The authorization is based on OAuth claims granted by an authorization server.
The granted claims are placed in a JWT access token within the "extension_roles"-claim.
Each claim value represents and grants access to a single role in DataHub.

As an example, the payload of an access token giving roles `Supporter` and `Accountant` will look as follows.

```Json
{
  "sub": "1234567890",
  "extension_roles": ["Supporter", "Accountant"]
}
```

## Authorization in Web Apps

Endpoint authorization in web apps is enforced by policy-based authorization (see <https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies>).
Every supported permission is configured as a role, using the built-in framework to ensure that the user is both authenticated and has the correct claim.
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
    [Authorize(Permission.Accountant, Permission.Supporter)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple roles (Accountant && Supporter), if an endpoint requires both of the specified roles.

```C#
    [HttpPost]
    [Authorize(Permission.Accountant)]
    [Authorize(Permission.Supporter)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```