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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.FunctionApp.Extensions;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware
{
    public sealed class JwtTokenMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ClaimsPrincipalContext _claimsPrincipalContext;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        public JwtTokenMiddleware(ClaimsPrincipalContext claimsPrincipalContext, IJwtTokenValidator jwtTokenValidator)
        {
            _claimsPrincipalContext = claimsPrincipalContext ?? throw new ArgumentNullException(nameof(claimsPrincipalContext));
            _jwtTokenValidator = jwtTokenValidator;
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

            var (isValid, claimsPrincipal) = await _jwtTokenValidator.ValidateTokenAsync(token).ConfigureAwait(false);

            if (!isValid)
            {
                FunctionContextHelper.SetErrorResponse(context);
                return;
            }

            _claimsPrincipalContext.ClaimsPrincipal = claimsPrincipal;

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
