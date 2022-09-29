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

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Energinet.DataHub.Core.App.WebApp.Middleware;

/// <summary>
/// This temporary middleware processes the 'extension_roles' claim from B2C.
/// Because the claim is custom, the roles are received as an escaped array and require post-processing.
/// This middleware can be removed once the true 'roles' claim is available.
/// </summary>
public sealed class ExtensionRolesClaimMiddleware : IMiddleware
{
    private const string RoleClaimName = "extension_roles";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var user = context.User;

        var claim = FindRoleClaim(user);
        if (claim == null)
        {
            return next(context);
        }

        var rolesAsClaims = TransformCustomRoleClaim(claim)
            .Select(role => new Claim(RoleClaimName, role.Trim()));

        user.AddIdentity(new ClaimsIdentity(rolesAsClaims, null, null, RoleClaimName));

        return next(context);
    }

    private static IEnumerable<string> TransformCustomRoleClaim(Claim claim)
    {
        return claim.Value
            .Replace("[", string.Empty)
            .Replace("]", string.Empty)
            .Replace("\"", string.Empty)
            .Split(',');
    }

    private static Claim? FindRoleClaim(ClaimsPrincipal user)
    {
        return user.Claims.SingleOrDefault(c => c.Type == RoleClaimName);
    }
}
