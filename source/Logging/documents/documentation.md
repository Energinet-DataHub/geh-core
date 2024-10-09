# Logging Middleware for Request and Response

Middleware logs request and response to external storage.

Request and response stream should be seekable for the middleware to read stream multiple times.

Implementation example:

```c#
builder.UseMiddleware<RequestResponseLoggingMiddleware>();
```

---

``` c#
Container.Register<RequestResponseLoggingMiddleware>(Lifestyle.Scoped);
```

---

``` c#
serviceCollection.AddScoped<IRequestResponseLogging>(
    _ => new RequestResponseLoggingBlobStorage(connectionString,
                                                containerName,
                                                ILogger<RequestResponseLoggingBlobStorage>));
```

## App Release notes

## Version 1.1.1

- Bumped patch version as pipeline file was updated.

## LoggingMiddleware

Middleware that adds logging scope to the request. It it supported for both Function app and Web app.

The behavior is specific for Application Insight logging provider. It adds custom properties to the request telemetry to make it easier to filter and search for logs.

### Usage

Install `Energinet.DataHub.Core.Logging.LoggingMiddleware` package.

**Registration for Function app:**

```c#
public static IHost ConfigureApplication()
{
    var host = new HostBuilder()
        .ConfigureFunctionsWorkerDefaults(ConfigureWorker)
        .ConfigureServices(ConfigureServices)
        .ConfigureLogging(ConfigureLogging)
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

private static void ConfigureLogging(ILoggingBuilder builder)
{
    builder.SetApplicationInsightLogLevel();
}
```

**Registration for Web app:**

``` c#
public void ConfigureServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddHttpLoggingScope("domain-name");
}

public void Configure(IApplicationBuilder app)
{
    app.UseLoggingScope();
}
```
