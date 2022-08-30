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
        public async Task Given_AccessTokenIsNotAToken_When_ValidateTokenAsync_Then_IsValidShouldBeFalse_And_ClaimsPrincipalShouldBeNull()
        {
            var sut = new JwtTokenValidator(Fixture.BackendAppOpenIdSettings);
            var accessToken = string.Empty;

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(accessToken);

            isValid.Should().BeFalse();
            claimsPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Given_ValidAccessToken_When_ValidateTokenAsync_Then_IsValidShouldBeTrue_And_ClaimsPrincipalShouldNotBeNull()
        {
            var sut = new JwtTokenValidator(Fixture.BackendAppOpenIdSettings);
            var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(authenticationResult.AccessToken);

            isValid.Should().BeTrue();
            claimsPrincipal.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_BackendAccessToken_And_FrontendOpenIdSetting_When_ValidateTokenAsync_Then_IsValidShouldBeFalse_And_ClaimsPrincipalShouldBeNull()
        {
            var sut = new JwtTokenValidator(new OpenIdSettings(
                Fixture.AuthorizationConfiguration.FrontendOpenIdConfigurationUrl,
                Fixture.AuthorizationConfiguration.FrontendApp.AppId));
            var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(authenticationResult.AccessToken);

            isValid.Should().BeFalse();
            claimsPrincipal.Should().BeNull();
        }
    }
}
