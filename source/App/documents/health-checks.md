# Health Checks Documentation

Guidelines on implementing health checks for Function App's and ASP.NET Core Web API's.

TODO: Describe "liveness" and "readyness" endpoints.

> For a full implementation, see [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

## Functions

### Preparing an Azure Function App project

1) Install this NuGet package:
   `Energinet.DataHub.Core.App.FunctionApp`

1) Add the following to a *ConfigureServices()* method in Program.cs:

   ```cs
    // Health check
    serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
    serviceCollection.AddHealthChecks()
        .AddLiveCheck();
   ```

1) Create a new class file as `System\HealthCheckEndpoint.cs` with the following content:

   ```cs
    public class HealthCheckEndpoint
    {
        public HealthCheckEndpoint(IHealthCheckEndpointHandler healthCheckEndpointHandler)
        {
            EndpointHandler = healthCheckEndpointHandler;
        }

        private IHealthCheckEndpointHandler EndpointHandler { get; }

        [Function("HealthCheck")]
        public Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "monitor/{endpoint}")]
            HttpRequestData httpRequest,
            string endpoint)
        {
            return EndpointHandler.HandleAsync(httpRequest, endpoint);
        }
    }
   ```

1) Allow anonymous access to the health checks endpoints.

    > In the `Charges` domain this is handled using the `JwtTokenWrapperMiddleware`.

### Add health checks for dependencies

Even though the following are implemented for ASP.NET Core, it also works for Azure Functions.

[AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks#health-checks)

## ASP.NET Core Web API

