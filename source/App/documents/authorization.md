# Authorization Documentation

The authorization is based on OAuth claims granted by an authorization server. The granted claims are placed in a JWT access token within the "roles"-claim. Each claim value represents and grants access to a single permission in DataHub.

As an example, the payload of an access token giving permissions `Organization` and `GridAreas` will look as follows.

```Json
{
  "sub": "<user-id>",
  "azp": "<actor-id>",
  "token": "<access-token-from-AD>",
  "membership": "fas",
  "roles": ["organization:view", "gridareas:manage"]
}
```

Domains can either validate the token using the provided middleware or use another OIDC approach. The OIDC configuration endpoint is https://app-webapi-markpart-[environment].azurewebsites.net/.well-known/openid-configuration.

## Security

> Each domain must also implement its own domain-specific actor authorization! The middleware ensures only that the token is valid and the permissions have been granted. Failure to do so may lead to escalation of privileges.

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
    [AuthorizeUser(Permission.Organization)]
    public async Task<IActionResult> GetExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (Organization || GridAreas), if an endpoint requires any of the specified permissions.

```C#
    [HttpPost]
    [AuthorizeUser(Permission.Organization, Permission.GridAreas)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

It is possible to combine multiple permissions (Organization && GridAreas), if an endpoint requires both of the specified permissions.

```C#
    [HttpPut]
    [AuthorizeUser(Permission.Organization)]
    [AuthorizeUser(Permission.GridAreas)]
    public async Task<IActionResult> DoExampleAsync()
    {
        ...
    }
```

## Adding New Permissions

`Permission` enum, located in Common.Security-package, contains the permissions used by DataHub. The enum functions as the single source of supported permissions.

Adding or editing permission can be done in three steps:

1) Add a new permission to `Permission` enum.
2) Add a claim entry for the permission to `PermissionsAsClaims` dictionary. Use a simple and concise value; the entry will be sent with every token.
3) Publish the package
4) Create a new branch in `geh-market-participant`, so the next steps can be completed in the same PR
5) In the new branch, add new entries into the marketpart database to add permissions details and what marketroles this permission is valid for.
6) Update the package in `geh-market-participant`.
7) Create a PR in `geh-market-participant` and wait for it to be completed.
8) (Optional) If the permission is needed to guard features in `greenforce-frontend`, add the claim entry to `libs\dh\shared\feature-authorization\src\lib\permission.ts`.

## Adding permission details to marketpart database

- Create a new branch on the <https://github.com/Energinet-DataHub/geh-market-participant> repository
- Add a new script to the following folders
  - `source\Energinet.DataHub.MarketParticipant.ApplyDBMigrationsApp\Scripts\T-001\Seed`
  - `source\Energinet.DataHub.MarketParticipant.ApplyDBMigrationsApp\Scripts\U-001\Seed`
  - `source\Energinet.DataHub.MarketParticipant.ApplyDBMigrationsApp\Scripts\U-002\Seed`
  - `source\Energinet.DataHub.MarketParticipant.ApplyDBMigrationsApp\Scripts\B-001\Seed`
  - `source\Energinet.DataHub.MarketParticipant.ApplyDBMigrationsApp\Scripts\B-002\Seed`
  - `source\Energinet.DataHub.MarketParticipant.ApplyDBMigrationsApp\Scripts\LocalDB\`
- The files follow a specific naming structure, be sure to adhere to the existing files
  - The format is "YYYYMMDDhhmm Description.sql" (Description can't contain numbers)
    - YYYY = Year (I.E. 2023)
    - MM = Month (I.E. 02)
    - DD = Day (I.E. 25)
    - hh = Hour (I.E. 18)
    - mm = Minute (I.E. 42)
- The file should contain the follwing script

  ```SQL
  INSERT INTO [dbo].[Permission]
           ([Id]
           ,[Description]
           ,[Created])
     VALUES
           (<Id, int,> -- This is the enum value of your new permission I.E. 1,2,3 etc.
           ,<Description, nvarchar(250),> -- This is the description of your new permission
           ,<Created, datetimeoffset>) -- The permission created date, used for history and audit log
  GO

  INSERT INTO [dbo].[PermissionEicFunction]
           ([Id]
           ,[PermissionId]
           ,[EicFunction])
     VALUES
           (<Id, uniqueidentifier,> -- GUID for this entry, just auto generate one
           ,<PermissionId, int,> -- This is the enum value of your new permission I.E. 1,2,3 etc.
           ,<EicFunction, int,>) -- This is the enum value of the EIC function I.E. 1,2,3 etc. this permission is linked to
  GO
  ```

- The last part of the script can be repeated for all the EIC Functions this permission should be linked to
- Values for the EIC Function enum can be found in `source/Energinet.DataHub.MarketParticipant.Domain/Model/EicFunction.cs`
- Verify that unit tests are succesfull, there is an integration test, that will ensure that you have added descriptions and marketroles to all permissions.
  - Note: this only checks for files in the `LocalDB` folder, so you are responsible for ensuring it has been copied to all other folders.
- Once the files are added, you should create a PR for your branch and once approved and merged, the new permission has been created with all needed information


### The permissions are now available for use. Please be aware of the following caveats:

- Remember to assign the newly added permission to a user role through the UI.
- It is safe to rename the permissions in the `Permission` enum.
- It is safe to rename the claim entries, but be aware that user will lose access to the permission until their token expires.
- Be CAUTIOUS when changing the numeric values of the `Permission` enum.
- It is NOT SAFE to reuse the numeric values of the `Permission` enum.
