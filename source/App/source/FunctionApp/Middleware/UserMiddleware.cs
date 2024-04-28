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
using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Users;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware;

/// <summary>
/// If possible, retrieved JWT from header and creates a user, which is then
/// added to the function context and thereby made available in the executing function.
/// </summary>
public class UserMiddleware<TUser> : IFunctionsWorkerMiddleware
    where TUser : class
{
    private const string MultiTenancyClaim = "multitenancy";

    // DO NOT inject scoped services in the middleware constructor.
    // DO use scoped services in middleware by retrieving them from 'FunctionContext.InstanceServices'
    // DO NOT store scoped services in fields or properties of the middleware object. See https://github.com/Azure/azure-functions-dotnet-worker/issues/1327#issuecomment-1434408603
    public UserMiddleware()
    {
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Retrieve any dependent services first, to fail fast is we have registration issues
        var logger = context.GetLogger<UserMiddleware<TUser>>();
        var userProvider = context.InstanceServices.GetRequiredService<IUserProvider<TUser>>();
        var userContext = context.InstanceServices.GetRequiredService<UserContext<TUser>>();

        try
        {
            var token = await TryGetTokenFromHeaderAsync(context).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (tokenHandler.CanReadToken(token))
                {
                    var securityToken = tokenHandler.ReadJwtToken(token);

                    var userId = GetUserId(securityToken.Claims);
                    var actorId = GetActorId(securityToken.Claims);
                    var multiTenancy = GetMultiTenancy(securityToken.Claims);

                    var user = await userProvider
                        .ProvideUserAsync(userId, actorId, multiTenancy, securityToken.Claims)
                        .ConfigureAwait(false);

                    // TODO: Should we at any point set the status code to Unauthorized (401), and skip calling any further middleware?
                    if (user != null)
                    {
                        userContext.SetCurrentUser(user);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing user object from token.");
        }

        await next(context).ConfigureAwait(false);
    }

    private static async Task<string> TryGetTokenFromHeaderAsync(FunctionContext context)
    {
        var token = string.Empty;

        var requestData = await context.GetHttpRequestDataAsync().ConfigureAwait(false);
        if (requestData!.Headers.TryGetValues("authorization", out var authorizationHeaders))
        {
            var authorizationHeader = authorizationHeaders.First();
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorizationHeader["Bearer ".Length..].Trim();
            }
        }

        return token;
    }

    private static Guid GetUserId(IEnumerable<Claim> claims)
    {
        // TODO: In Web App the "Sub" claim is shown as "ClaimTypes.NameIdentifier" - not sure why
        var userId = claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
        return Guid.Parse(userId);
    }

    private static Guid GetActorId(IEnumerable<Claim> claims)
    {
        var actorId = claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Azp).Value;
        return Guid.Parse(actorId);
    }

    private static bool GetMultiTenancy(IEnumerable<Claim> claims)
    {
        return claims.Any(claim => claim is { Type: MultiTenancyClaim, Value: "true" });
    }
}
