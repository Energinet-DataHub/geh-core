# Middleware Documentation

Middleware for Function App's and ASP.NET Core Web API's.

## JWT Token Middleware

### Introduction

When this middleware is added, it will be required to apply a valid JWT token to the header of the HTTP request. The token will automatically be validated.

If token is not applied or could not be validated, **401** is returned to the client.

Following claim must exist in the applied Bearer token

- azp (Actor ID)

### Usage

Install following packages

- `Energinet.DataHub.Core.App.Common`
- `Energinet.DataHub.Core.App.Common.Abstractions`

And follow the instructions for either Functions or WebApis below. After those steps `ClaimsPrincipal` can now be accessed through `IClaimsPrincipalAccessor`.

#### Functions

Install `Energinet.DataHub.Core.App.FunctionApp`.

Add Middleware to `ConfigureFunctionsWorkerDefaults` as **the first in line** as below:

```c#
.ConfigureFunctionsWorkerDefaults(options => options
{
    options.UseMiddleware<JwtTokenMiddleware>();
    …
})
```

Register in IoC (in example below SimpleInjector is used)
Note: The following package must be installed

- `Energinet.DataHub.Core.App.FunctionApp.SimpleInjector`

```c#
protected override void ConfigureContainer(Container container)
{
    …
    container.AddJwtTokenSecurity("https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", "audience")
    …
}
```

#### WebApi

Install the following

- `Energinet.DataHub.Core.App.WebApp`.
- `Energinet.DataHub.Core.App.WebApp.SimpleInjector`

Replace the default middleware factory with the SimpleInjectorMiddlewareFactory:

```c#
services.AddTransient<IMiddlewareFactory>(_ =>
{
    return new SimpleInjectorMiddlewareFactory(_container);
});
```

If SimpleInjector is not already in use, do remember to wrap ASP.NET Core requests in a SimpleInjector execution context:

```c#
services.UseSimpleInjectorAspNetRequestScoping(_container);
```

Register in container:

```c#
…
container.AddJwtTokenSecurity("https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration", "audience")
…
```

## Actor Middleware

### Introduction

This middleware extends functionality from [JWT Token Middleware](#jwt-token-middleware).

When this middleware is added, it will be possible to get the current actor via IActorContext. The context will be populated based on the id from the token and the IActorProvider. You must provide an implementation of IActorProvider.

### Usage

Setup following the same steps as for `JwtTokenMiddleware`, using the same packages as before.

Add `ActorMiddleware` after `JwtTokenMiddleware` as below:

```c#
options.UseMiddleware<JwtTokenMiddleware>();
options.UseMiddleware<ActorMiddleware>();
…
```

Register in container:

```c#
…
container.AddActorContext<MyActorProviderImplementation>()
…
```

`CurrentActor` can now be accessed through `IActorContext`.

## Integration Event Metadata Middleware

### Introduction

This middleware provides functionality for reading intergration event metadata for ServiceBus events. When this middleware is added, it will be possible to get the integration event metadata via IIntegrationEventContext.

### Usage

Add `IntegrationEventMetadataMiddleware` as below:

```c#
options.UseMiddleware<IntegrationEventMetadataMiddleware>();
…
```

Register in container:

```c#
…
container.Register<IIntegrationEventContext, IntegrationEventContext>(Lifestyle.Scoped);
container.Register<IntegrationEventMetadataMiddleware>(Lifestyle.Scoped);
…
```

Integration event metadata can now be accessed through `IIntegrationEventContext`.
