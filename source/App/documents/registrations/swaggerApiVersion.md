# Swagger and Api versioning

ApiVersion is a library that allows for easy versioning of the API.
And together with Swagger we can visualize this in a easy way.
The swagger UI will show the different versions of the APIs and allow for easy testing of the different versions.
Currently we only have swagger implemented for ASP.NET Core Web API.

## Overview

- Implementation
    - [ASP.NET Core Web API](#aspnet-core-web-api)

## ASP.NET Core Web API

After following the guidelines below, one should have a functional web api project with a simple swagger UI with default version for all endpoints.

### Preparing a Web App project

1) Install this NuGet package: `Energinet.DataHub.Core.App.WebApp`

2) Make sure that you have `<GenerateDocumentationFile>true</GenerateDocumentationFile>` enabled in building props.

3) Add the following to Program.cs (minimal hosting model):

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

To overwrite the version of a method/controller, one may set the following attribute to the method/controller:

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

Here the request to url: `https://...` will hit `GetVersion2`, `https://...?api-version=2` will hit `GetVersion2`.
and `https://...?api-version=1` will hit `GetVersion1`.

#### Deprecating a version

If every `[ApiVersion(1.0)]` tag is marked as deprecated: `[ApiVersion(1.0, Deprecated = true)]` then the swagger UI will
show an short description underneath the title and mark "V1" as deprecated in the dropdown menu in the top right.

Furthermore, methods marked with `[Obsolete]` will have a strikethrough and be grayed out, in the swagger UI.
They will still be interactable.

#### Decoration the swagger UI with the method documentation

When `GenerateDocumentationFile` is set to `true`, there will be generated a xml file,
containing method documentations and similarly.
By utilizing this file, one can add the methods documentation to the swagger UI.

By adding the following to the `.csproj` file,

```csharp
  <Target Name="PrepublishScript" BeforeTargets="PrepareForPublish">
    <ItemGroup>
      <DocFile Include="bin\*\*\*.xml" />
    </ItemGroup>
    <Copy SourceFiles="@(DocFile)"
          DestinationFolder="$(PublishDir)"
          SkipUnchangedFiles="false" />
  </Target>
```

the xml documentation will be copied to the
publish directory. Swagger will use this XML file to add the documentation of the methods to them, in the UI.
