# Authorization Documentation

The authorization is based on OAuth scopes granted by an authorization server.
The granted scopes are placed in a JWT access token within the "scope"-claim.
Each scope value represents and grants access to a single permission in DataHub.

As an example, the payload of an access token giving permissions `OrganizationRead` and `OrganizationWrite` will look as follows.

```Json
{
  "sub": "1234567890",
  "scope": ["organization:read", "organization:write"]
}
```

## Authorization in Web Apps

Endpoint authorization in web apps is enforced by policy-based authorization (see <https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies>).
Every supported permission is configured as a policy, using the built-in framework to ensure that the user is both authenticated and has the correct claim.
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

1) Add `UseAuthorization()` adter `UseAuthentication()` to `IApplicationBuilder`, see <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization>.
2) Add `AddPermissionAuthorization()` to `IServiceProvider`. This will register the permissions with the framework.

### Usage

This package includes an `AuthorizeAttribute` for selecting a supported permission.
The attribute can be used to annotate Controller classes or individual methods within.

For example, if an endpoint requires 'OrganizationRead' permission, the attribute can be used as follows.

```C#
    [HttpGet]
    [Authorize(Permission.OrganizationRead)]
    public async Task<IActionResult> GetExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (OrganizationRead ^ GridAreaWrite), if an endpoint requires both permissions to complete its task.

```C#
    [HttpPost]
    [Authorize(Permission.OrganizationRead)]
    [Authorize(Permission.GridAreaWriteRead)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

Multiple choice permissions (OrganizationRead | GridAreaWrite) are not supported; neither by the framework, nor the underlying model.
