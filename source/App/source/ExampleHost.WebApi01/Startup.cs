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
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExampleHost.WebApi01
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // CONCLUSION:
            //  * Logging using ILogger<T> will work, but notice that by default we need to log as "Warning" for it to appear in Application Insights (can be configured).
            //    See "How do I customize ILogger logs collection" at https://docs.microsoft.com/en-us/azure/azure-monitor/faq#how-do-i-customize-ilogger-logs-collection-

            // CONCLUSION:
            //  * We can see Trace, Request, Dependencies and other entries in App Insights out-of-box.
            //    See https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
            services.AddApplicationInsightsTelemetry();

            services.AddHttpClient(HttpClientNames.WebApi02, httpClient =>
            {
                var baseUrl = Configuration.GetValue<string>(EnvironmentSettingNames.WebApi02BaseUrl);
                httpClient.BaseAddress = new Uri(baseUrl!);
            });

            services.AddHostedService<SomeTrigger>();
            services.AddSingleton<SomeTrigger.SomeWorker>();
            // Ensure we register the "Thrower" if we start the hot locally, but not if this was already registered by tests.
            services.TryAddSingleton<SomeTrigger.SomeWorker.Thrower>();

            services
                .AddHealthChecks()
                .AddLiveCheck()
                .AddRepeatingTriggerHealthCheck<SomeTrigger>(TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // We will not use HTTPS in tests. For correct enforcement of HTTPS see: https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio
            ////app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapLiveHealthChecks();
                endpoints.MapReadyHealthChecks();
            });
        }
    }
}
