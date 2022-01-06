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
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Extensions;
using Energinet.DataHub.Core.FunctionApp.Common.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.FunctionApp.Common.Middleware
{
    public sealed class JwtTokenMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IUserContext _userContext;
        private readonly bool _validateToken;
        private readonly JwtSecurityTokenHandler _tokenValidator;

        public JwtTokenMiddleware(IUserContext userContext)
        {
            _userContext = userContext;
            _validateToken = false; // NOTE: For now we should not perform token validation
            _tokenValidator = new JwtSecurityTokenHandler();
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!TryGetTokenFromHeaders(context, out var token))
            {
                // Unable to get token from headers
                SetErrorResponse(context);
                return;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // Token is malformed
                SetErrorResponse(context);
                return;
            }

            try
            {
                UserIdentity? userIdentity;
                if (_validateToken)
                {
                    // Validate token
                    var principal = _tokenValidator.ValidateToken(token, new TokenValidationParameters(), out _);
                    userIdentity = UserIdentityFactory.FromClaimsPrincipal(principal);
                }
                else
                {
                    var securityToken = _tokenValidator.ReadJwtToken(token);
                    userIdentity = UserIdentityFactory.FromJwtSecurityToken(securityToken);
                }

                _userContext.CurrentUser = userIdentity;
            }
            catch (SecurityTokenException)
            {
                // Token is not valid (expired etc.)
                SetErrorResponse(context);
                return;
            }

            await next(context).ConfigureAwait(false);
        }

        private static void SetErrorResponse(FunctionContext context)
        {
            var httpRequestData = context.GetHttpRequestData() ?? throw new InvalidOperationException();
            var httpResponseData = httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);

            context.SetHttpResponseData(httpResponseData);
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
