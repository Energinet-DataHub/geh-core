# Azure AD B2C token retrieval

In DataHub we use Azure AD B2C to build a cloud identity directory. Users and client applications are created as entities in this directory. Users can access the UI (frontend) if given access to the frontend application. Client applications can access the API if given access to the backend application.

In the authentication and authorization of users and client applications we use JWT's. As the external interface is http based, any incoming http requests requires an access token. To be able to perform integration or system tests targeting http requests, we therefore need to be able to retrieve an access token similar to a user or client application.

The `B2CAuthorizationConfiguration` makes it easy to retrieve settings we have stored in a key vault, which are required in the process of retrieving and validating access tokens.

The `B2CAppAuthenticationClient` makes it easy, using settings retrieved by `B2CAuthorizationConfiguration`, to retrieve an access token for a client application targeting a backend application.

> For usage, see `B2CFixture` or [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

## Example

In the following example we will show how we can retrieve an access token that allows the client application (here `endk-tso`) access to the backend application (the DataHub API) in the development environment (here `u001`). This access token must then later be added to http requests from the client application.

To test using the `endk-tso` client application in the `u001` enviroments we first initialize the `B2CAuthorizationConfiguration` like:

```csharp
var environment = "u001";
var systemOperator = "endk-tso";

var authorizationConfiguration = new B2CAuthorizationConfiguration(
    usedForSystemTests: false,
    environment: environment,
    new List<string> { systemOperator });
```

Then, using the retrieved settings, we can initialize the `B2CAppAuthenticationClient` like:

```csharp
var backendAppAuthenticationClient = new B2CAppAuthenticationClient(
    authorizationConfiguration.TenantId,
    authorizationConfiguration.BackendApp,
    authorizationConfiguration.ClientApps[systemOperator]);
```

Finally we can use the `B2CAppAuthenticationClient` to retrieve an access token like:

```csharp
var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();
var accessToken = authenticationResult.AccessToken;
```
