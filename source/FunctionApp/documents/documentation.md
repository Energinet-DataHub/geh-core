# Azure Function Common Documentation

A library containing common functionality for Azure Functions.

## JWT Token Middleware

### Introduction

When this middleware is added, it will be required to apply a valid JWT token to the header of the HTTP request. The token will automatically be validated.

If token is not applied or could not be validated, **401** is returned to the client.

Following claim must exist in the applied Bearer token

* azp (Actor ID)

### Usage

Install following packages

* `Energinet.DataHub.Core.FunctionApp.Common`
* `Energinet.DataHub.Core.FunctionApp.Common.Abstractions`
  
Add Middleware to `ConfigureFunctionsWorkerDefaults` as **the first in line** as below:

```c#
.ConfigureFunctionsWorkerDefaults(options => options
{
    options.UseMiddleware<JwtTokenMiddleware>();
    ...
})
```

Register in IoC (in example below SimpleInjector is used)
Note: The following package must be installed
* `Energinet.DataHub.Core.FunctionApp.Common.SimpleInjector`

```c#
protected override void ConfigureContainer(Container container)
{
    container.AddJwtTokenSecurity("https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", "audience")
    ...
}
```

`ClaimsPrincipal` can now be accessed through `IClaimsPrincipalAccessor`
