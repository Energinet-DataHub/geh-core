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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.App.Common.Security
{
    public class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly JwtSecurityTokenHandler _tokenValidator;
        private readonly OpenIdSettings _openIdSettings;

        public JwtTokenValidator(OpenIdSettings openIdSettings)
        {
            _tokenValidator = new JwtSecurityTokenHandler();
            _openIdSettings = openIdSettings ?? throw new ArgumentNullException(nameof(openIdSettings));
        }

        public async Task<(bool IsValid, ClaimsPrincipal? ClaimsPrincipal)> ValidateTokenAsync(string? token)
        {
            if (!_tokenValidator.CanReadToken(token))
            {
                // Token is malformed
                return (false, null);
            }

            try
            {
                var openIdConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(_openIdSettings.MetadataAddress, new OpenIdConnectConfigurationRetriever());
                var openIdConnectConfigData = await openIdConfigurationManager.GetConfigurationAsync();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = _openIdSettings.Audience,
                    IssuerSigningKeys = openIdConnectConfigData.SigningKeys,
                    ValidIssuer = openIdConnectConfigData.Issuer,
                };

                var principal = _tokenValidator.ValidateToken(token, validationParameters, out _);

                return (true, principal);
            }
            catch (Exception)
            {
                // Token is not valid (expired etc.)
                return (false, null);
            }
        }
    }
}
