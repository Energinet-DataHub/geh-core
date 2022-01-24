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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Extensions;
using Energinet.DataHub.Core.FunctionApp.Common.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Middleware.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.FunctionApp.Common.Middleware
{
    public sealed class JwtTokenMiddleware : IFunctionsWorkerMiddleware
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

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var httpRequestData = context.GetHttpRequestData();
            if (httpRequestData == null)
            {
                await next(context).ConfigureAwait(false);
                return;
            }

            if (!TryGetTokenFromHeaders(context, out var token))
            {
                // Unable to get token from headers
                FunctionContextHelper.SetErrorResponse(context);
                return;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // Token is malformed
                FunctionContextHelper.SetErrorResponse(context);
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
                FunctionContextHelper.SetErrorResponse(context);
                return;
            }

            await next(context).ConfigureAwait(false);
        }

        private static bool TryGetTokenFromHeaders(FunctionContext context, out string? token)
        {
            token = null;

            // HTTP headers are in the binding context as a JSON object
            // The first checks ensure that we have the JSON string
            if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
            {
                return false;
            }

            if (headersObj is not string headersStr)
            {
                return false;
            }

            // Deserialize headers from JSON
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersStr);

            if (headers == null) return false;

            var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
            if (!normalizedKeyHeaders.TryGetValue("authorization", out var authHeaderValue))
            {
                // No Common header present
                return false;
            }

            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // Scheme is not Bearer
                return false;
            }

            token = authHeaderValue.Substring("Bearer ".Length).Trim();
            return true;
        }
    }
}
