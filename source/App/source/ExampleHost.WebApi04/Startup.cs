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

using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using ExampleHost.WebApi04.Security;

namespace ExampleHost.WebApi04;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Http => Authentication (verified in tests)
        // Configure for testing
        AuthenticationExtensions.DisableHttpsConfiguration = true;
        services
            .AddJwtBearerAuthenticationForWebApp(Configuration)
            .AddUserAuthenticationForWebApp<ExampleSubsystemUser, ExampleSubsystemUserProvider>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // We will not use HTTPS in tests.
        app.UseRouting();

        // Configuration supporting tested scenarios
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseUserMiddlewareForWebApp<ExampleSubsystemUser>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
