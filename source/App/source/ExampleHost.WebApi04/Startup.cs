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

namespace ExampleHost.WebApi04;

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

        // Configuration supporting tested scenarios
        var mitIdInnerMetadata = _configuration["mitIdInnerMetadata"]!;
        var innerMetadata = _configuration["innerMetadata"]!;
        var outerMetadata = _configuration["outerMetadata"]!;
        var audience = _configuration["audience"]!;

        AuthenticationExtensions.DisableHttpsConfiguration = true;

        AddJwtAuthentication(services, mitIdInnerMetadata, innerMetadata, outerMetadata, audience);
        services.AddUserAuthentication<ExampleDomainUser, ExampleDomainUserProvider>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // We will not use HTTPS in tests.
        app.UseRouting();

        // Configuration supporting tested scenarios
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseUserMiddleware<ExampleDomainUser>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    protected virtual void AddJwtAuthentication(IServiceCollection services, string mitIdInnerMetadata, string innerMetadata, string outerMetadata, string audience)
    {
        services.AddJwtBearerAuthentication(innerMetadata, outerMetadata, audience);
    }
}
