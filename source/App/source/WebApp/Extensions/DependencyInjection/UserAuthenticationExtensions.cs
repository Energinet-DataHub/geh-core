﻿// Copyright 2020 Energinet DataHub A/S
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

using System.IdentityModel.Tokens.Jwt;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.App.Common.Users;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding authentication services to an ASP.NET Core app.
/// </summary>
public static class UserAuthenticationExtensions
{
    private const string ExternalTokenClaimType = "token";

    public static IServiceCollection AddUserAuthenticationForWebApp<TUser, TUserProvider>(this IServiceCollection services)
        where TUser : class
        where TUserProvider : class, IUserProvider<TUser>
    {
        // UserMiddleware depends on IHttpContextAccessor.
        services.AddHttpContextAccessor();

        services.AddScoped<UserContext<TUser>>();
        services.AddScoped<IUserContext<TUser>>(s => s.GetRequiredService<UserContext<TUser>>());
        services.AddScoped<IUserProvider<TUser>, TUserProvider>();
        services.AddScoped<UserMiddleware<TUser>>();

        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication to the Web API.
    ///
    /// Expects <see cref="UserAuthenticationOptions"/> has been configured in <see cref="UserAuthenticationOptions.SectionName"/>.
    /// </summary>
    public static IServiceCollection AddJwtBearerAuthenticationForWebApp(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var authenticationOptions = configuration
            .GetRequiredSection(UserAuthenticationOptions.SectionName)
            .Get<UserAuthenticationOptions>();

        if (authenticationOptions == null)
            throw new InvalidConfigurationException("Missing authentication configuration.");

        GuardAuthenticationOptions(authenticationOptions);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                IEnumerable<TokenValidationParameters> tokenValidationParameters =
                [
                    CreateValidationParameters(authenticationOptions.BackendBffAppId, authenticationOptions.MitIdExternalMetadataAddress),
                    CreateValidationParameters(authenticationOptions.BackendBffAppId, authenticationOptions.ExternalMetadataAddress),
                ];

                options.TokenValidationParameters = CreateValidationParameters(authenticationOptions.BackendBffAppId, authenticationOptions.InternalMetadataAddress);

                // Notes regarding "IssuerValidatorUsingConfiguration":
                //  - We must have a dependency to "Microsoft.AspNetCore.Authentication.JwtBearer" otherwise the validation workflow
                //    won't perform at call to get the configurations (Issuer and Keys) and then 'configuration' will be null.
                //  - We should keep this code and its dependency in synch with the code in the 'FunctionApp' project.
                options.TokenValidationParameters.IssuerValidatorUsingConfiguration = (issuer, token, _, configuration) =>
                {
                    if (configuration == null)
                        throw new InvalidOperationException("The 'Configuration' is null. Either JwtBearer dependencies are missing or we could not retrieve the 'Configuration' from the configured metadata address.");
                    if (!string.Equals(configuration.Issuer, issuer, StringComparison.Ordinal))
                        throw new SecurityTokenInvalidIssuerException { InvalidIssuer = issuer };

                    foreach (var param in tokenValidationParameters)
                    {
                        if (TryValidateExternalJwt((JsonWebToken)token, param))
                            return issuer;
                    }

                    throw new UnauthorizedAccessException("Internal token could not be validated");
                };
            });

        return services;
    }

    private static void GuardAuthenticationOptions(UserAuthenticationOptions authenticationOptions)
    {
        if (string.IsNullOrWhiteSpace(authenticationOptions.MitIdExternalMetadataAddress))
            throw new InvalidConfigurationException($"Missing '{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}'.");
        if (string.IsNullOrWhiteSpace(authenticationOptions.ExternalMetadataAddress))
            throw new InvalidConfigurationException($"Missing '{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}'.");
        if (string.IsNullOrWhiteSpace(authenticationOptions.BackendBffAppId))
            throw new InvalidConfigurationException($"Missing '{nameof(UserAuthenticationOptions.BackendBffAppId)}'.");
        if (string.IsNullOrWhiteSpace(authenticationOptions.InternalMetadataAddress))
            throw new InvalidConfigurationException($"Missing '{nameof(UserAuthenticationOptions.InternalMetadataAddress)}'.");
    }

    private static TokenValidationParameters CreateValidationParameters(
        string audience,
        string metadataAddress)
    {
        return new TokenValidationParameters
        {
            ValidAudience = audience,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true }),
        };
    }

    private static void ValidateExternalJwt(JsonWebToken internalToken, TokenValidationParameters tokenValidationParameters)
    {
        var externalTokenClaim = internalToken.Claims.Single(claim =>
            string.Equals(claim.Type, ExternalTokenClaimType, StringComparison.OrdinalIgnoreCase));

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(externalTokenClaim.Value, tokenValidationParameters, out _);
    }

    private static bool TryValidateExternalJwt(JsonWebToken internalToken, TokenValidationParameters tokenValidationParameters)
    {
        try
        {
            ValidateExternalJwt(internalToken, tokenValidationParameters);
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }
}
