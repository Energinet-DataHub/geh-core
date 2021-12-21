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
using System.Security.Claims;
using Energinet.DataHub.Core.FunctionApp.Common.Identity;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public class UserIdentityTests
    {
        [Fact]
        public void Should_return_user_identity_from_claims_principal()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity[]
            {
                new(GetClaims(guid)),
            });

            // Act
            var userIdentity = UserIdentityFactory.FromClaimsPrincipal(claimsPrincipal);

            // Assert
            userIdentity.Should().NotBeNull();
            userIdentity?.ActorId.Should().Be(guid);
            userIdentity?.Role.Should().Be("roles");
            userIdentity?.IdentifierType.Should().Be("eic");
            userIdentity?.Identifier.Should().Be("identifier");
        }

        [Fact]
        public void Should_return_user_identity_from_jwt_security_token()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var jwtSecurityToken = new JwtSecurityToken(null, null, GetClaims(guid));

            // Act
            var userIdentity = UserIdentityFactory.FromJwtSecurityToken(jwtSecurityToken);

            // Assert
            userIdentity.Should().NotBeNull();
            userIdentity?.ActorId.Should().Be(guid);
            userIdentity?.Role.Should().Be("roles");
            userIdentity?.IdentifierType.Should().Be("eic");
            userIdentity?.Identifier.Should().Be("identifier");
        }

        private static IEnumerable<Claim> GetClaims(Guid guid)
        {
            return new List<Claim>
            {
                new(CustomClaimTypes.ActorId, guid.ToString()), new(CustomClaimTypes.Roles, "roles"), new(CustomClaimTypes.IdentifierType, "eic"), new(CustomClaimTypes.Identifier, "identifier"),
            };
        }
    }
}
