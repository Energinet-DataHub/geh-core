# Connector

The `Connector` folder contains classes and methods that provide functionality for configuring and using service endpoints in an application. This includes handling authentication, caching, and retry policies for HTTP requests.

## Capabilities

The `Connector` provides the following capabilities:
- **Service Endpoint Configuration**: Allows you to configure service endpoints using configuration files.
- **Authentication**: Handles authentication for HTTP requests using Azure Identity.
- **Caching**: Caches access tokens to improve performance.
- **Retry Policies**: Implements retry policies for handling transient HTTP errors.

## How to Register

To register the service endpoint in your application, you need to add the following code to your `Startup.cs` or `Program.cs` file:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    services.AddServiceEndpoint<MyServiceEndpoint>(configuration.GetSection("MyServiceEndpoint"));
}
```

## How to Use - with HttpClientFactory

Once registered, you can use the service endpoint in your application code as follows:

```csharp
public class MyService
{
    private readonly HttpClient _httpClient;

    public MyService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.GetHttpClient<MyServiceEndpoint>();
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

## How to Use - without HttpClientFactory

Once registered, you can use the generic ServiceEndpoint in your application code as follows:

```csharp
public class MyService
{
    private readonly ServiceEndpoint<MyServiceEndpoint> _serviceEndpoint;

    public MyService(ServiceEndpoint<MyServiceEndpoint> serviceEndpoint)
    {
        _serviceEndpoint = serviceEndpoint;
    }

    public async Task<string> GetDataAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/data");
        var response = await _serviceEndpoint.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

## Configuration File Example

Here is an example of how to configure the service endpoint in your `appsettings.json` file:

```json
{
  "MyServiceEndpoint": {
    "BaseAddress": "https://api.example.com",
    "Identity": {
      "ApplicationIds": [ "your-application-id" ]
    }
  }
}
```

This configuration specifies the base address for the service endpoint and the application ID for authentication.