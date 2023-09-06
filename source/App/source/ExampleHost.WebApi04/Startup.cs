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

using Energinet.DataHub.Core.App.WebApp.Authentication;
using ExampleHost.WebApi04.Security;

namespace ExampleHost.WebApi04
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            var innerMetadata = _configuration["innerMetadata"]!;
            var outerMetadata = _configuration["outerMetadata"]!;
            var audience = _configuration["audience"]!;

            AuthenticationExtensions.DisableHttpsConfiguration = true;

            services.AddControllers();
            services.AddJwtBearerAuthentication(innerMetadata, outerMetadata, audience);
            services.AddUserAuthentication<ExampleDomainUser, ExampleDomainUserProvider>();
            services.AddApplicationInsightsTelemetry();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // We will not use HTTPS in tests. For correct enforcement of HTTPS see: https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio
            ////app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseUserMiddleware<ExampleDomainUser>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
