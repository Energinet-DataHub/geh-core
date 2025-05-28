// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using Asp.Versioning;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using ExampleHost.WebApi01.Common;
using ExampleHost.WebApi01.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;

namespace ExampleHost.WebApi01;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Configuration verified in tests:
        //  * Logging using ILogger<T> will work, but notice that by default we need to log as "Warning" for it to
        //    appear in Application Insights (can be configured).
        //    See "How do I customize ILogger logs collection" at https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcorenew#how-do-i-customize-ilogger-logs-collection
        //  * We can see Trace, Request, Dependencies and other entries in App Insights out-of-box.
        //    See https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
        //  * Telemetry events are enriched with property "Subsystem" and configured value
        services.AddApplicationInsightsForWebApp(subsystemName: "ExampleHost.WebApi");

        // Configure token credential provider to share token credential
        //  * Used by "AddAuthorizationHeaderProvider"
        //  * Can be used when registering access to Azure resources (e.g. service bus etc.)
        services.AddTokenCredentialProvider();

        // Health Checks (verified in tests)
        services.AddHealthChecksForWebApp();
        services
            .AddHealthChecks()
            .AddCheck("verify-ready", () => HealthCheckResult.Healthy())
            .AddCheck("verify-status", () => HealthCheckResult.Healthy(), tags: [HealthChecksConstants.StatusHealthCheckTag]);

        // Swagger and api versioning (verified in tests)
        services
            .AddSwaggerForWebApp(
                Assembly.GetExecutingAssembly(),
                swaggerUITitle: "ExampleHost.WebApi",
                swaggerUIDescription: "This is the API for ExampleHost.WebApi",
                useFullnameForSchemaIds: true)

            // Setting default version to 2.0, this will be overwritten if the method has it's own version
            .AddApiVersioningForWebApp(new ApiVersion(2, 0));

        // Feature management (verified in tests)
        services
            .AddAzureAppConfiguration()
            .AddFeatureManagement();

        // Http => Client side subsystem-to-subsystem authentication (verified in tests)
        //  * Depends on services registered by "AddTokenCredentialProvider"
        services
            .AddAuthorizationHeaderProvider()
            .AddWebApi02HttpClient();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // Configuration verified in tests:
        //  * Enable automatic feature flag refresh on each http request
        //  * Must be placed before "UseEndpoints"
        app.UseAzureAppConfiguration();

        // We will not use HTTPS in tests.
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            // Health Checks (verified in tests)
            endpoints.MapLiveHealthChecks();
            endpoints.MapReadyHealthChecks();
            endpoints.MapStatusHealthChecks();
        });

        // Swagger (verified in tests)
        app.UseSwaggerForWebApp();
    }
}
