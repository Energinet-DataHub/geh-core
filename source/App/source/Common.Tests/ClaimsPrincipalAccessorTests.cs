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
using System.Linq;
using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Identity;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests
{
    public class ClaimsPrincipalAccessorTests
    {
        [Fact]
        public void Should_return_user_identity_from_claims_principal()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var claimsPrincipalContext = new ClaimsPrincipalContext
            {
                ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(CreateClaims(guid))),
            };
            var sut = new ClaimsPrincipalAccessor(claimsPrincipalContext);

            // Act
            var claimsPrincipal = sut.GetClaimsPrincipal();
            var claim = GetClaim(claimsPrincipal!);

            // Assert
            claim.Value.Should().NotBeNull();
            claim.Value.Should().Be(guid.ToString());
        }

        private static IEnumerable<Claim> CreateClaims(Guid guid)
        {
            return new List<Claim> { new("azp", guid.ToString()), };
        }

        private static Claim GetClaim(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Single(x => x.Type == "azp");
        }
    }
}
