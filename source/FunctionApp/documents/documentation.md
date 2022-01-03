# Azure Function Common Documentation

A library containing common functionality for Azure Functions.

## JWT Token Middleware

### Introduction

When this middleware is added, it will automatically try to extract a Bearer token from the header of a HTTP request.

If applied, it will be transformed to a `UserIdentity` which is exposed through `IUserContext`.

If no token are applied or it was not possible to transform **401** are returned to the client.

Following claims must exist in the applied Bearer token

* ActorId
* Roles
* IdentifierType (EIC, GLN, etc.)
* Identifier (Actual value of identifier)

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

```c#
protected override void ConfigureContainer(Container container)
{
    container.Register<JwtTokenMiddleware>(Lifestyle.Scoped);
    container.Register<IUserContext, UserContext>(Lifestyle.Scoped);
    ...
}
```

An instance of `UserIdentity` is now available through `IUserContext`
