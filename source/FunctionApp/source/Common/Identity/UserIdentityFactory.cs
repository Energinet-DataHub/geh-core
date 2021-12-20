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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;

namespace Energinet.DataHub.Core.FunctionApp.Common.Identity
{
    public static class UserIdentityFactory
    {
        public static UserIdentity? FromClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
        {
            return CreateFromClaims(claimsPrincipal.Claims.ToList());
        }

        public static UserIdentity? FromJwtSecurityToken(JwtSecurityToken securityToken)
        {
            return CreateFromClaims(securityToken.Claims.ToList());
        }

        private static UserIdentity? CreateFromClaims(IReadOnlyCollection<Claim> claims)
        {
            var actorIdClaim = GetClaim(claims, CustomClaimTypes.ActorId);

            if (!Guid.TryParse(actorIdClaim?.Value, out var id))
            {
                return null;
            }

            var rolesClaim = GetClaim(claims, CustomClaimTypes.Roles);
            if (rolesClaim == null) return null;

            var identifierTypeClaim = GetClaim(claims, CustomClaimTypes.IdentifierType); // gln | eic
            if (identifierTypeClaim == null) return null;

            var identifierClaim = GetClaim(claims, CustomClaimTypes.Identifier); // GLN / EIC specific identifiers

            return identifierClaim == null ? null : new UserIdentity(id, rolesClaim.Value, identifierTypeClaim.Value, identifierClaim.Value);
        }

        private static Claim? GetClaim(IEnumerable<Claim> claims, string claimType)
        {
            return claims.SingleOrDefault(x => x.Type == claimType);
        }
    }
}
