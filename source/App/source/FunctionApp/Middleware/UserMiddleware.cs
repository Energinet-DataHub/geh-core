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

using System.Net;
using System.Security.Claims;
using DarkLoop.Azure.Functions.Authorization;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware;

/// <summary>
/// This middleware is only supported for HttpTrigger functions that uses ASP.NET Core integration for HTTP.
/// It depends on types registered by DarkLoop Authorization extension and expects DarkLoop middleware has
/// been executed earlier in the middleware pipeline.
///
/// If possible, retrieved JWT from header and creates a user, which is then
/// set to the <see cref="UserContext{TUser}"/> and thereby made available
/// for dependency injection in the executing function and other services.
/// </summary>
public class UserMiddleware<TUser> : IFunctionsWorkerMiddleware
    where TUser : class
{
    private const string MultiTenancyClaim = "multitenancy";

    private readonly IFunctionsAuthorizationProvider _authorizationProvider;
    private readonly IAuthorizationPolicyProvider _policyProvider;

    // DO NOT inject scoped services in the middleware constructor.
    // DO use scoped services in middleware by retrieving them from 'FunctionContext.InstanceServices'
    // DO NOT store scoped services in fields or properties of the middleware object. See https://github.com/Azure/azure-functions-dotnet-worker/issues/1327#issuecomment-1434408603
    public UserMiddleware(
        IFunctionsAuthorizationProvider authorizationProvider,
        IAuthorizationPolicyProvider policyProvider)
    {
        _authorizationProvider = authorizationProvider;
        _policyProvider = policyProvider;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext()
            ?? throw new InvalidOperationException("UserMiddleware running without HttpContext. ASP.NET Core integration for HTTP is required.");

        var filter = await _authorizationProvider.GetAuthorizationAsync(context.FunctionDefinition.Name, _policyProvider).ConfigureAwait(false);
        if (filter.AllowAnonymous)
        {
            // Next middleware
            await next(context).ConfigureAwait(false);
            return;
        }

        var isUserSet = await TrySetUserAsync(context, httpContext.Request).ConfigureAwait(false);
        if (isUserSet)
        {
            // Next middleware
            await next(context).ConfigureAwait(false);
            return;
        }

        // Subsystem did not accept the user or we could not create the user.
        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }

    private static async Task<bool> TrySetUserAsync(FunctionContext context, HttpRequest httpRequest)
    {
        var logger = context.GetLogger<UserMiddleware<TUser>>();
        var userProvider = context.InstanceServices.GetRequiredService<IUserProvider<TUser>>();
        var userContext = context.InstanceServices.GetRequiredService<UserContext<TUser>>();

        try
        {
            var token = TryGetTokenFromHeader(httpRequest);
            if (!string.IsNullOrWhiteSpace(token))
            {
                var tokenHandler = new JsonWebTokenHandler();
                if (tokenHandler.CanReadToken(token))
                {
                    var securityToken = (JsonWebToken)tokenHandler.ReadToken(token);

                    var userId = GetUserId(securityToken.Claims);
                    var actorId = GetActorId(securityToken.Claims);
                    var multiTenancy = GetMultiTenancy(securityToken.Claims);

                    var user = await userProvider
                        .ProvideUserAsync(userId, actorId, multiTenancy, securityToken.Claims)
                        .ConfigureAwait(false);

                    if (user != null)
                    {
                        userContext.SetCurrentUser(user);
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user context from token.");
        }

        return false;
    }

    private static string TryGetTokenFromHeader(HttpRequest httpRequest)
    {
        var token = string.Empty;

        // We only accept one Authorization header
        if (httpRequest.Headers.Authorization.Count == 1)
        {
            var authorizationHeader = httpRequest.Headers.Authorization.ToString();
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorizationHeader["Bearer ".Length..].Trim();
            }
        }

        return token;
    }

    private static Guid GetUserId(IEnumerable<Claim> claims)
    {
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
