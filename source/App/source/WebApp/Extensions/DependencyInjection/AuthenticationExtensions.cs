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
using Energinet.DataHub.Core.App.Common.Users;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding authentication services to an ASP.NET Core app.
/// </summary>
public static class AuthenticationExtensions
{
    private const string InnerTokenClaimType = "token";

    /// <summary>
    /// Disables HTTPS requirement for OpenId configuration endpoints.
    /// This property is intended for testing purposes only and we use InternalsVisibleTo in the project file to control who can access it.
    /// </summary>
    internal static bool DisableHttpsConfiguration { get; set; }

    // TODO: Add description
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
    /// </summary>
    /// <param name="services">A collection of service descriptors.</param>
    /// <param name="externalMetadataAddress">The address of OpenId configuration endpoint for the external token, e.g. https://{b2clogin.com/tenant-id/policy}/v2.0/.well-known/openid-configuration.</param>
    /// <param name="internalMetadataAddress">The address of OpenId configuration endpoint for the internal token, e.g. https://{market-participant-web-api}/.well-known/openid-configuration.</param>
    /// <param name="backendAppId"></param>
    public static IServiceCollection AddJwtBearerAuthenticationForWebApp(
        this IServiceCollection services,
        string externalMetadataAddress,
        string internalMetadataAddress,
        string backendAppId)
    {
        ArgumentNullException.ThrowIfNull(externalMetadataAddress);
        ArgumentNullException.ThrowIfNull(backendAppId);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var tokenValidationParameters = CreateValidationParameters(backendAppId, externalMetadataAddress);

                if (!string.IsNullOrEmpty(internalMetadataAddress))
                {
                    options.TokenValidationParameters = CreateValidationParameters(backendAppId, internalMetadataAddress);
                    options.TokenValidationParameters.IssuerValidatorUsingConfiguration = (issuer, token, _, configuration) =>
                    {
                        if (!string.Equals(configuration.Issuer, issuer, StringComparison.Ordinal))
                            throw new SecurityTokenInvalidIssuerException { InvalidIssuer = issuer };

                        ValidateInnerJwt((JsonWebToken)token, tokenValidationParameters);
                        return issuer;
                    };
                }
                else
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                }
            });

        return services;
    }

    /// <summary>
    /// Adds JWT Bearer authentication to the Web API.
    /// </summary>
    /// <param name="services">A collection of service descriptors.</param>
    /// <param name="mitIdExternalMetadataAddress">The address of MitID configuration endpoint for the external token.</param>
    /// <param name="externalMetadataAddress">The address of OpenId configuration endpoint for the external token, e.g. https://{b2clogin.com/tenant-id/policy}/v2.0/.well-known/openid-configuration.</param>
    /// <param name="internalMetadataAddress">The address of OpenId configuration endpoint for the internal token, e.g. https://{market-participant-web-api}/.well-known/openid-configuration.</param>
    /// <param name="backendAppId"></param>
    public static IServiceCollection AddJwtBearerAuthenticationForWebApp(
        this IServiceCollection services,
        string mitIdExternalMetadataAddress,
        string externalMetadataAddress,
        string internalMetadataAddress,
        string backendAppId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mitIdExternalMetadataAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalMetadataAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(internalMetadataAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(backendAppId);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                IEnumerable<TokenValidationParameters> tokenValidationParameters =
                [
                    CreateValidationParameters(backendAppId, mitIdExternalMetadataAddress),
                    CreateValidationParameters(backendAppId, externalMetadataAddress),
                ];

                options.TokenValidationParameters = CreateValidationParameters(backendAppId, internalMetadataAddress);
                options.TokenValidationParameters.IssuerValidatorUsingConfiguration = (issuer, token, _, configuration) =>
                {
                    if (!string.Equals(configuration.Issuer, issuer, StringComparison.Ordinal))
                        throw new SecurityTokenInvalidIssuerException { InvalidIssuer = issuer };

                    foreach (var param in tokenValidationParameters)
                    {
                        if (TryValidateInnerJwt((JsonWebToken)token, param))
                            return issuer;
                    }

                    throw new UnauthorizedAccessException("Internal token could not be validated");
                };
            });

        return services;
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
                new HttpDocumentRetriever { RequireHttps = !DisableHttpsConfiguration }),
        };
    }

    private static void ValidateInnerJwt(JsonWebToken outerToken, TokenValidationParameters tokenValidationParameters)
    {
        var innerTokenClaim = outerToken.Claims.Single(claim =>
            string.Equals(claim.Type, InnerTokenClaimType, StringComparison.OrdinalIgnoreCase));

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(innerTokenClaim.Value, tokenValidationParameters, out _);
    }

    private static bool TryValidateInnerJwt(JsonWebToken outerToken, TokenValidationParameters tokenValidationParameters)
    {
        try
        {
            ValidateInnerJwt(outerToken, tokenValidationParameters);
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }
}
