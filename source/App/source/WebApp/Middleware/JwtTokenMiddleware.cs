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

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.WebApp.Middleware.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.App.WebApp.Middleware
{
    public sealed class JwtTokenMiddleware : IMiddleware
    {
        private readonly JwtSecurityTokenHandler _tokenValidator;
        private readonly ClaimsPrincipalContext _claimsPrincipalContext;
        private readonly OpenIdSettings _configuration;

        public JwtTokenMiddleware(ClaimsPrincipalContext claimsPrincipalContext, OpenIdSettings settings)
        {
            _tokenValidator = new JwtSecurityTokenHandler();
            _claimsPrincipalContext = claimsPrincipalContext ?? throw new ArgumentNullException(nameof(claimsPrincipalContext));
            _configuration = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!TryGetTokenFromHeaders(context, out var token))
            {
                // Unable to get token from headers
                HttpContextHelper.SetErrorResponse(context);
                return;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // Token is malformed
                HttpContextHelper.SetErrorResponse(context);
                return;
            }

            try
            {
                var openIdConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(_configuration.MetadataAddress, new OpenIdConnectConfigurationRetriever());
                var openIdConnectConfigData = await openIdConfigurationManager.GetConfigurationAsync();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = _configuration.Audience,
                    IssuerSigningKeys = openIdConnectConfigData.SigningKeys,
                    ValidIssuer = openIdConnectConfigData.Issuer,
                };

                var principal = _tokenValidator.ValidateToken(token, validationParameters, out _);
                _claimsPrincipalContext.ClaimsPrincipal = principal;
            }
            catch (Exception)
            {
                // Token is not valid (expired etc.)
                HttpContextHelper.SetErrorResponse(context);
                return;
            }

            await next(context).ConfigureAwait(false);
        }

        private static bool TryGetTokenFromHeaders(HttpContext context, out string? token)
        {
            token = null;

            // HTTP headers are in the binding context as a JSON object
            // The first checks ensure that we have the JSON string
            if (!context.Request.Headers.TryGetValue("Authorization", out var authValues))
            {
                return false;
            }

            if (authValues == StringValues.Empty)
            {
                return false;
            }

            var authValue = authValues.ToString();

            if (!authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Scheme is not Bearer
                return false;
            }

            token = authValue.Substring("Bearer ".Length).Trim();
            return true;
        }
    }
}
