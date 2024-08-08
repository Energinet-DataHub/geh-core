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
using System.Security.Claims;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;

/// <summary>
/// A JWT provider used for creating internal JWT tokens for testing DH3 applications that require authentication and authorization.
/// Can be used with <see cref="OpenIdMockServer"/> for creating JWT tokens that are valid according to the OpenId server.
/// </summary>
public class JwtProvider
{
    private const string TokenClaim = "token";
    private const string RoleClaim = "role";

    private readonly AzureB2CSettings _azureB2CSettings;
    private readonly string _issuer;
    private readonly RsaSecurityKey _securityKey;

    /// <summary>
    /// Create a JWT provider
    /// </summary>
    /// <param name="azureB2CSettings">Azure B2C settings used to get an external token from Microsoft Entra. Should be retrieved from <see cref="IntegrationTestConfiguration"/></param>
    /// <param name="issuer">The JWT issuer used for the JWT. If using <see cref="OpenIdMockServer"/> then the issuer should be the same.</param>
    /// <param name="securityKey">The security key used for signing the JWT. If using <see cref="OpenIdMockServer"/> then the security key should be the same.</param>
    internal JwtProvider(AzureB2CSettings azureB2CSettings, string issuer, RsaSecurityKey securityKey)
    {
        _azureB2CSettings = azureB2CSettings;
        _issuer = issuer;
        _securityKey = securityKey;
    }

    /// <summary>
    /// The appllication id of the client app registration in Microsoft Entra. The App id is the client application on
    /// which behalf the external token is retrieved from Microsoft Entra.
    /// This is not the actual BFF but a test app registration that allows us to verify some of the JWT code.
    /// </summary>
    public string TestBffAppId => _azureB2CSettings.TestBffAppId;

    /// <summary>
    /// The authority url is the url from where the external token is retrieved
    /// </summary>
    internal string ExternalTokenAuthorityUrl => $"https://login.microsoftonline.com/{_azureB2CSettings.Tenant}";

    /// <summary>
    /// Creates an internal token valid for DataHub3 applications, containing the following claims:
    /// - "token" claim which is an external token retrieved from Microsoft Entra (configured in the given <see cref="AzureB2CSettings"/>)
    /// - "sub" claim specified in the <paramref name="userId"/> parameter
    /// - "azp" claim specified in the <paramref name="actorId"/> parameter
    /// - "role" claims for each role specified in the <paramref name="roles"/> parameter
    /// - Any extra claims specified in the <paramref name="extraClaims"/> parameter
    /// </summary>
    /// <param name="userId">The user id value written to the 'sub' claim in the internal token.</param>
    /// <param name="actorId">The actor id value written to the 'azp' claim in the internal token.</param>
    /// <param name="roles">Optional roles to add as "role" claims in the internal token. When running in Azure this could be something like "calculations:manage".</param>
    /// <param name="extraClaims">Optional extra claims to add to the internal token.</param>
    /// <returns>The internal token which wraps the provided external token.</returns>
    public async Task<string> CreateInternalTokenAsync(
        string userId = "A1AAB954-136A-444A-94BD-E4B615CA4A78", // TODO: Is it possible to override these, or are they bound to the external token?
        string actorId = "A1DEA55A-3507-4777-8CF3-F425A6EC2094", // TODO: Is it possible to override these, or are they bound to the external token?
        string[]? roles = null,
        Claim[]? extraClaims = null)
    {
        roles ??= [];
        extraClaims ??= [];

        var externalTokenResult = await GetExternalTokenAsync().ConfigureAwait(false);
        var externalToken = externalTokenResult.AccessToken;

        var claims = new List<Claim>
        {
            new(TokenClaim, externalToken),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Azp, actorId),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(RoleClaim, role.Trim()));
        }

        claims.AddRange(extraClaims);

        var externalJwt = new JwtSecurityToken(externalToken);
        var internalJwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: externalJwt.Audiences.Single(),
            claims: claims,
            notBefore: externalJwt.ValidFrom,
            expires: externalJwt.ValidTo,
            signingCredentials: new SigningCredentials(_securityKey, SecurityAlgorithms.RsaSha256));

        var internalToken = new JwtSecurityTokenHandler().WriteToken(internalJwt);
        return internalToken;
    }

    /// <summary>
    /// Get an external JWT from Microsoft Entra using the given <see cref="AzureB2CSettings"/>
    /// </summary>
    private Task<AuthenticationResult> GetExternalTokenAsync()
    {
        var confidentialClientApp = ConfidentialClientApplicationBuilder
            .Create(_azureB2CSettings.ServicePrincipalId)
            .WithClientSecret(_azureB2CSettings.ServicePrincipalSecret)
            .WithAuthority(authorityUri: ExternalTokenAuthorityUrl)
            .Build();

        return confidentialClientApp
            .AcquireTokenForClient(scopes: new[] { $"{_azureB2CSettings.TestBffAppId}/.default" })
            .ExecuteAsync();
    }
}
