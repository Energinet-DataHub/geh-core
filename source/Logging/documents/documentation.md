# Logging Middleware for Request and Response

Middleware logs request and response to external storage.

Request and response stream should be seekable for the middleware to read stream multiple times.

Implementation example:

    builder.UseMiddleware<RequestResponseLoggingMiddleware>();
---

    Container.Register<RequestResponseLoggingMiddleware>(Lifestyle.Scoped);
---

    serviceCollection.AddScoped<IRequestResponseLogging>(
    _ => new RequestResponseLoggingBlobStorage(connectionString,
                                                containerName,
                                                ILogger<RequestResponseLoggingBlobStorage>));

## App Release notes

## Version 1.1.1

- Bumped patch version as pipeline file was updated.

## LoggingScopeMiddleware

### Usage

Install `Energinet.DataHub.Core.Logging.LoggingScopeMiddleware` package.

**Registration for Function app:**

```c#
public static IHost ConfigureApplication()
{
    var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults(ConfigureWorker)
        .ConfigureServices(ConfigureServices)
        .Build();
    return host;
}

private static void ConfigureWorker(IFunctionsWorkerApplicationBuilder builder)
{
    builder.UseLoggingScope();
}

private static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
{
    serviceCollection.AddFunctionLoggingScope("domain-name");
}
```

**Registration for Web app:**

```c#
public void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddHttpLoggingScope("domain-name");
}

public void Configure(IApplicationBuilder app)
{
    app.UseLoggingScope();
}
```