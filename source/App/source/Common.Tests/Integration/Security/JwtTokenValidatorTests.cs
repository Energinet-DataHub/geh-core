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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.Common.Tests.Fixtures;
using FluentAssertions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Integration.Security
{
    public class JwtTokenValidatorTests : IClassFixture<B2CFixture>
    {
        public JwtTokenValidatorTests(B2CFixture fixture)
        {
            Fixture = fixture;

            SecurityTokenValidator = new JwtSecurityTokenHandler();
            BackendOpenIdConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                Fixture.AuthorizationConfiguration.BackendOpenIdConfigurationUrl,
                new OpenIdConnectConfigurationRetriever());
        }

        private B2CFixture Fixture { get; }

        private ISecurityTokenValidator SecurityTokenValidator { get; }

        private IConfigurationManager<OpenIdConnectConfiguration> BackendOpenIdConfigurationManager { get; }

        [Fact]
        public async Task Given_AccessTokenIsNotAToken_When_ValidateTokenAsync_Then_IsValidShouldBeFalse_And_ClaimsPrincipalShouldBeNull()
        {
            var sut = new JwtTokenValidator(
                SecurityTokenValidator,
                BackendOpenIdConfigurationManager,
                Fixture.AuthorizationConfiguration.BackendApp.AppId);
            var accessToken = string.Empty;

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(accessToken);

            isValid.Should().BeFalse();
            claimsPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Given_ValidAccessToken_When_ValidateTokenAsync_Then_IsValidShouldBeTrue_And_ClaimsPrincipalShouldNotBeNull()
        {
            var sut = new JwtTokenValidator(
                SecurityTokenValidator,
                BackendOpenIdConfigurationManager,
                Fixture.AuthorizationConfiguration.BackendApp.AppId);
            var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(authenticationResult.AccessToken);

            isValid.Should().BeTrue();
            claimsPrincipal.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_BackendAccessToken_And_FrontendAudience_When_ValidateTokenAsync_Then_IsValidShouldBeFalse_And_ClaimsPrincipalShouldBeNull()
        {
            var sut = new JwtTokenValidator(
                SecurityTokenValidator,
                BackendOpenIdConfigurationManager,
                Fixture.AuthorizationConfiguration.FrontendApp.AppId);
            var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(authenticationResult.AccessToken);

            isValid.Should().BeFalse();
            claimsPrincipal.Should().BeNull();
        }

        [Fact]
        public async Task Given_BackendAccessToken_And_IssuerSigningKeyHasBeenRemovedUsingMock_When_ValidateTokenAsync_Then_ConfigurationIsRefreshed_And_IsValidShouldBeTrue_And_ClaimsPrincipalShouldNotBeNull()
        {
            var openIdConfigurationManagerMock = new ConfigurationManagerMock(BackendOpenIdConfigurationManager);
            var sut = new JwtTokenValidator(
                SecurityTokenValidator,
                openIdConfigurationManagerMock,
                Fixture.AuthorizationConfiguration.BackendApp.AppId);
            var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();

            // Act
            var (isValid, claimsPrincipal) = await sut.ValidateTokenAsync(authenticationResult.AccessToken);

            isValid.Should().BeTrue();
            claimsPrincipal.Should().NotBeNull();
        }

        /// <summary>
        /// This mock is used to test the scenario where a token is signed using a key which
        /// is no longer cached in the configuration manager. This should be detected and the
        /// configuration should be refreshed, after which we should try the validation again.
        /// Its important that we only try this once, so we don't end up in an endless loop.
        /// </summary>
        private class ConfigurationManagerMock : IConfigurationManager<OpenIdConnectConfiguration>
        {
            public ConfigurationManagerMock(IConfigurationManager<OpenIdConnectConfiguration> innerConfigurationManager)
            {
                InnerConfigurationManager = innerConfigurationManager;
                ClearSigningKeys = true;
            }

            private IConfigurationManager<OpenIdConnectConfiguration> InnerConfigurationManager { get; }

            private bool ClearSigningKeys { get; set; }

            public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
            {
                var actualConfiguration = await InnerConfigurationManager.GetConfigurationAsync(CancellationToken.None);

                // Clear SigningKeys the first time current method is called.
                if (ClearSigningKeys)
                {
                    ClearSigningKeys = false;

                    // Remove all SigningKeys to provoke an exception during validation.
                    actualConfiguration.SigningKeys.Clear();
                }

                return actualConfiguration;
            }

            public void RequestRefresh()
            {
                InnerConfigurationManager.RequestRefresh();
            }
        }
    }
}
