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
using System.Net;
using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Energinet.DataHub.Core.App.WebApp.Middleware;

public sealed class UserMiddleware<TUser> : IMiddleware
    where TUser : class
{
    private const string MultiTenancyClaim = "multitenancy";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserProvider<TUser> _userProvider;
    private readonly UserContext<TUser> _userContext;

    public UserMiddleware(
        IHttpContextAccessor httpContextAccessor,
        IUserProvider<TUser> userProvider,
        UserContext<TUser> userContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _userProvider = userProvider;
        _userContext = userContext;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("UserMiddleware running without HttpContext.");

        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var claimsPrincipal = httpContext.User;
        var userId = GetUserId(claimsPrincipal.Claims);
        var actorId = GetActorId(claimsPrincipal.Claims);
        var multiTenancy = GetMultiTenancy(claimsPrincipal.Claims);

        var user = await _userProvider
            .ProvideUserAsync(userId, actorId, multiTenancy, claimsPrincipal.Claims)
            .ConfigureAwait(false);

        // Subsystem did not accept the user; returns 401.
        if (user == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        _userContext.SetCurrentUser(user);
        await next(context).ConfigureAwait(false);
    }

    private static Guid GetUserId(IEnumerable<Claim> claims)
    {
        var userId = claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
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
