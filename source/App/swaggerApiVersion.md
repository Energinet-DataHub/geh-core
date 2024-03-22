# Swagger and Api versioning

## Overview
- [Introduction](#introduction)
- Implementation
  - [ASP.NET Core Web API](#aspnet-core-web-api) 


## Introduction
Swagger together with ApiVersioning is a powerful tool to document and version the API.

## ASP.NET Core Web API
After following the guidelines below, one should have a functional web api project with a simple swagger UI with default version for all endpoints.

### Preparing a Web App project
1) Install this NuGet package:
      `Energinet.DataHub.Core.App.WebApp`
2) Add the following to Program.cs (minimal hosting model):

    ```cs
    builder.Services
               .AddSwaggerForWebApp(Assembly.GetExecutingAssembly(), swaggerUiTitle: $"{Title to dislay in swagger ui}")
               .AddApiVersioningForWebApp(new ApiVersion(1, 0));
   ```
   and
    ```cs
    app.UseSwaggerForWebApp();
   ```
   to the web application.

    This will setup a default swagger UI for the web app with the specified title and every method will default to version 1.0.
    The version is required but may be ignored, since it does not change anything for the general implementation and urls.

### Additional configuration

#### Overwriting the version of a method/class
It is possible to overwrite the api version of a method/class, which is by default the version set in 
```csharp
builder.Services.AddApiVersioningForWebApp(new ApiVersion(1, 0));
```
To overwrite the version of a method/class, one may set the following attribute to the method/class:
```cs
[ApiVersion("2.0")]
```
Which will specify that the method or all methods in the class, has version 2.0.
Likewise it is possible to have different versions for the same url.

```cs
[ApiVersion("1.0")]
[HttpGet]
public IActionResult GetVersion1()
{...}

[ApiVersion("2.0")]
[HttpGet]
public IActionResult GetVersion2()
{...}
 ```
Here the request to url: `https://...` will hit `GetVersion2`. 'https://...?api-version=2' will hit `GetVersion2`.
and 'https://...?api-version=1' will hit `GetVersion1`.

#### Deprecating a version
If every `[ApiVersion(1.0)]` tag is marked as deprecated: `[ApiVersion(1.0, Deprecated = true)]` then the swagger UI will 
show an short description underneath the title and mark "V1" as deprecated in the dropdown menu.

Furthermore, methods mars with `[Obsolete]` will have a strikethrough and grayed out.
But they will still be interactable. 

#### Decoration the swagger UI with the method documentation