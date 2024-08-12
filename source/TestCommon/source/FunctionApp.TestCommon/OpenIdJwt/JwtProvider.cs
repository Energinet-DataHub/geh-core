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
internal class JwtProvider : IJwtProvider
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
    /// The full URL of the configuration metadata endpoint which should be used to
    /// get the OpenId configuration required to verify the external token.
    /// </summary>
    public string ExternalMetadataAddress => $"{ExternalTokenAuthorityUrl}/{OpenIdMockServer.ConfigurationEndpointPath}";

    /// <summary>
    /// The authority url is the url from where the external token is retrieved
    /// </summary>
    private string ExternalTokenAuthorityUrl => $"https://login.microsoftonline.com/{_azureB2CSettings.Tenant}";

    public async Task<string> CreateInternalTokenAsync(
        string userId, // TODO: Is it possible to override these, or are they bound to the external token?
        string actorId, // TODO: Is it possible to override these, or are they bound to the external token?
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

    public string CreateFakeToken()
    {
        var securityKey = new SymmetricSecurityKey("not-a-secret-key-not-a-secret-key"u8.ToArray());
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userIdAsSubClaim = new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString());
        var actorIdAsAzpClaim = new Claim(JwtRegisteredClaimNames.Azp, Guid.NewGuid().ToString());

        var securityToken = new JwtSecurityToken(claims: [userIdAsSubClaim, actorIdAsAzpClaim], signingCredentials: credentials);
        var fakeToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return fakeToken;
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
