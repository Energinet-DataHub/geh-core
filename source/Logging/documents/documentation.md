# Logging Middleware for Request and Response

Middleware logs request and response to external storage.

Request and response stream should be seekable for the middleware to read stream multiple times.

Implementation example:


    builder.UseMiddleware<RequestResponseLoggingMiddleware>();
---

    Container.Register<RequestResponseLoggingMiddleware>(Lifestyle.Scoped);
---

    serviceCollection.AddScoped<IRequestResponseLogging>(
    _ => new RequestResponseLoggingBlobStorage(accountName, 
                                                containerName, 
                                                ILogger<RequestResponseLoggingBlobStorage>));

## App Release notes

## Version 1.1.1

- Bumped patch version as pipeline file was updated.