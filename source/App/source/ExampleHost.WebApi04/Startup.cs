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
using Energinet.DataHub.Core.App.WebApp.Extensions.Options;
using ExampleHost.WebApi04.Security;
using Microsoft.IdentityModel.Protocols.Configuration;

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

        // Configure for testing
        AuthenticationExtensions.DisableHttpsConfiguration = true;

        AddJwtAuthentication(services);
        services.AddUserAuthenticationForWebApp<ExampleDomainUser, ExampleDomainUserProvider>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // We will not use HTTPS in tests.
        app.UseRouting();

        // Configuration supporting tested scenarios
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseUserMiddlewareForWebApp<ExampleDomainUser>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    /// <summary>
    /// Here we configure the application using the "old" (obsolete) extension, for testing scenarious where we don't have nested tokens.
    /// </summary>
    protected virtual void AddJwtAuthentication(IServiceCollection services)
    {
        var authenticationOptions = Configuration
            .GetRequiredSection(AuthenticationOptions.SectionName)
            .Get<AuthenticationOptions>();

        if (authenticationOptions == null)
            throw new InvalidConfigurationException("Missing authentication configuration.");

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddJwtBearerAuthenticationForWebApp(
            authenticationOptions.ExternalMetadataAddress,
            authenticationOptions.InternalMetadataAddress,
            authenticationOptions.BackendBffAppId);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
