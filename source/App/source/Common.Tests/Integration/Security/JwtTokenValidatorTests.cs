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

using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.Common.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Integration.Security
{
    public class JwtTokenValidatorTests : IClassFixture<B2CFixture>
    {
        public JwtTokenValidatorTests(B2CFixture fixture)
        {
            Fixture = fixture;
        }

        private B2CFixture Fixture { get; }

        [Fact]
        public async Task Given_ValidToken_When_CallingValidateTokenAsync_Then_IsValidAndClaimsPrincipalIsNotNull()
        {
            var tokenResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();

            var openIdSettings = new OpenIdSettings(
                $"https://login.microsoftonline.com/{Fixture.AuthorizationConfiguration.TenantId}/v2.0/.well-known/openid-configuration",
                Fixture.BackendAppAuthenticationClient.AppSettings.AppId);
            var sut = new JwtTokenValidator(openIdSettings);

            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(tokenResult.AccessToken);

            isValid.Should().BeTrue();
            claimsPrincipal.Should().NotBeNull();
        }
    }
}
