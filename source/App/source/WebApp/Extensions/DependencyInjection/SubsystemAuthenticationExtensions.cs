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

using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding authentication services to a Web App
/// for subsystem-to-subsystem communication.
/// </summary>
public static class SubsystemAuthenticationExtensions
{
    /// <summary>
    /// Register services necessary for enabling a Web App
    /// to use JWT Bearer authentication for endpoints used in subsystem-to-subsystem communication.
    /// </summary>
    /// <remarks>
    /// Expects <see cref="SubsystemAuthenticationOptions"/> has been configured in <see cref="SubsystemAuthenticationOptions.SectionName"/>.
    /// </remarks>
    public static IServiceCollection AddSubsystemAuthenticationForWebApp(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var authenticationOptions = configuration
            .GetRequiredSection(SubsystemAuthenticationOptions.SectionName)
            .Get<SubsystemAuthenticationOptions>();

        if (authenticationOptions == null)
            throw new InvalidOperationException("Missing subsystem authentication configuration.");

        GuardAuthenticationOptions(authenticationOptions);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Audience = authenticationOptions.ApplicationIdUri;
                options.Authority = authenticationOptions.Issuer;
            });

        return services;
    }

    private static void GuardAuthenticationOptions(SubsystemAuthenticationOptions authenticationOptions)
    {
        if (string.IsNullOrWhiteSpace(authenticationOptions.ApplicationIdUri))
            throw new InvalidConfigurationException($"Missing '{nameof(SubsystemAuthenticationOptions.ApplicationIdUri)}'.");
        if (string.IsNullOrWhiteSpace(authenticationOptions.Issuer))
            throw new InvalidConfigurationException($"Missing '{nameof(SubsystemAuthenticationOptions.Issuer)}'.");
    }
}
