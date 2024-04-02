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

using System.IdentityModel.Tokens.Jwt;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Users;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.App.WebApp.Authentication;

public static class AuthenticationExtensions
{
    private const string InnerTokenClaimType = "token";

    /// <summary>
    /// Disables HTTPS requirement for OpenId configuration endpoints.
    /// This property is intended for testing purposes only.
    /// </summary>
    public static bool DisableHttpsConfiguration { get; set; }

    public static void UseUserMiddleware<TUser>(this IApplicationBuilder builder)
        where TUser : class
    {
        builder.UseMiddleware<UserMiddleware<TUser>>();
    }

    public static void AddUserAuthentication<TUser, TUserProvider>(this IServiceCollection services)
        where TUser : class
        where TUserProvider : class, IUserProvider<TUser>
    {
        // UserMiddleware depends on IHttpContextAccessor.
        services.AddHttpContextAccessor();

        services.AddScoped<UserContext<TUser>>();
        services.AddScoped<IUserContext<TUser>>(s => s.GetRequiredService<UserContext<TUser>>());
        services.AddScoped<IUserProvider<TUser>, TUserProvider>();
        services.AddScoped<UserMiddleware<TUser>>();
    }

    /// <summary>
    /// Adds JWT Bearer authentication to the Web API.
    /// </summary>
    /// <param name="services">A collection of service descriptors.</param>
    /// <param name="externalMetadataAddress">The address of OpenId configuration endpoint for the external token, e.g. https://{b2clogin.com/tenant-id/policy}/v2.0/.well-known/openid-configuration.</param>
    /// <param name="internalMetadataAddress">The address of OpenId configuration endpoint for the internal token, e.g. https://{market-participant-web-api}/.well-known/openid-configuration.</param>
    /// <param name="backendAppId"></param>
    public static void AddJwtBearerAuthentication(
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
                        {
                            throw new SecurityTokenInvalidIssuerException { InvalidIssuer = issuer };
                        }

                        ValidateInnerJwt((JsonWebToken)token, tokenValidationParameters);
                        return issuer;
                    };
                }
                else
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                }
            });
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
}
