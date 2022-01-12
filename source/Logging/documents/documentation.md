# Logging Middleware for Request and Response

Middleware logs request and response to external storage.

Request and response stream should be seekable for the middleware to read stream multiple times.

Implementation example:

+ builder.UseMiddleware<'RequestResponseLoggingMiddleware>();

+ serviceCollection.AddScoped<IRequestResponseLogging>(_ => new RequestResponseLoggingBlobStorage(string, string));
