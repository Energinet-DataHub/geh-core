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
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.App.Common.Security
{
    public class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly ILogger<JwtTokenValidator> _logger;
        private readonly ISecurityTokenValidator _securityTokenValidator;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _openIdConfigurationManager;
        private readonly string _validAudience;

        public JwtTokenValidator(
            ILogger<JwtTokenValidator> logger,
            ISecurityTokenValidator securityTokenValidator,
            IConfigurationManager<OpenIdConnectConfiguration> openIdConfigurationManager,
            string validAudience)
        {
            _logger = logger;
            _securityTokenValidator = securityTokenValidator;
            _openIdConfigurationManager = openIdConfigurationManager;
            _validAudience = validAudience;
        }

        public async Task<(bool IsValid, ClaimsPrincipal? ClaimsPrincipal)> ValidateTokenAsync(string? token)
        {
            if (!_securityTokenValidator.CanReadToken(token))
            {
                // Token is malformed
                return (false, null);
            }

            try
            {
                var claimsPrincipal = await ValidateTokenUsingConfigurationAsync(token);
                return (true, claimsPrincipal);
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                // Refresh configuration and try once more
                _logger.LogInformation("Force refreshing OpenID configuration because of missing signing key.");
                _openIdConfigurationManager.RequestRefresh();
            }
            catch (Exception)
            {
                // Token is not valid (expired etc.)
                return (false, null);
            }

            try
            {
                var claimsPrincipal = await ValidateTokenUsingConfigurationAsync(token);
                return (true, claimsPrincipal);
            }
            catch (Exception)
            {
                // Token is not valid (expired etc.)
                return (false, null);
            }
        }

        private async Task<ClaimsPrincipal> ValidateTokenUsingConfigurationAsync(string? token)
        {
            var openIdConnectConfiguration = await _openIdConfigurationManager.GetConfigurationAsync(CancellationToken.None);

            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = _validAudience,
                IssuerSigningKeys = openIdConnectConfiguration.SigningKeys,
                ValidIssuer = openIdConnectConfiguration.Issuer,
            };

            return _securityTokenValidator.ValidateToken(token, validationParameters, out _);
        }
    }
}
