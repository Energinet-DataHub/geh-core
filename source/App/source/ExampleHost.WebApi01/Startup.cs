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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using ExampleHost.WebApi01.Common;

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

        // CONCLUSION:
        //  * Logging using ILogger<T> will work, but notice that by default we need to log as "Warning" for it to appear in Application Insights (can be configured).
        //    See "How do I customize ILogger logs collection" at https://docs.microsoft.com/en-us/azure/azure-monitor/faq#how-do-i-customize-ilogger-logs-collection-

        // Configuration verified in tests
        // CONCLUSION:
        //  * We can see Trace, Request, Dependencies and other entries in App Insights out-of-box.
        //    See https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
        services.AddApplicationInsightsTelemetry();

        // Configure HttpClient for calling WebApi02
        services.AddHttpClient(HttpClientNames.WebApi02, httpClient =>
        {
            var baseUrl = _configuration.GetValue<string>(EnvironmentSettingNames.WebApi02BaseUrl);
            httpClient.BaseAddress = new Uri(baseUrl!);
        });

        // Health Checks (verified in tests)
        services
            .AddHealthChecks()
            .AddLiveCheck();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // We will not use HTTPS in tests.
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            // Health Checks (verified in tests)
            endpoints.MapLiveHealthChecks();
            endpoints.MapReadyHealthChecks();
        });
    }
}
