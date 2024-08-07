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

    private const string SubValue = "A1AAB954-136A-444A-94BD-E4B615CA4A78";
    private const string AzpValue = "A1DEA55A-3507-4777-8CF3-F425A6EC2094";

    private readonly string _issuer;
    private readonly RsaSecurityKey _securityKey;

    /// <summary>
    /// Create a JWT provider
    /// </summary>
    /// <param name="issuer">The JWT issuer. If using <see cref="OpenIdMockServer"/> then the issuer should be the same.</param>
    /// <param name="securityKey">The security key used for signing the JWT. If using <see cref="OpenIdMockServer"/> then the security key should be the same.</param>
    internal JwtProvider(string issuer, RsaSecurityKey securityKey)
    {
        _issuer = issuer;
        _securityKey = securityKey;
    }

    /// <summary>
    /// Creates an internal token valid for DataHub applications, containing the following claims:
    /// - "token" claim specified by the <paramref name="externalToken"/> parameter
    /// - "role" claims for each role specified in the <paramref name="roles"/> parameter
    /// - "sub" claim with the value "A1AAB954-136A-444A-94BD-E4B615CA4A78"
    /// - "azp" claim with the value "A1DEA55A-3507-4777-8CF3-F425A6EC2094"
    /// </summary>
    /// <param name="externalToken">Value of the "token" claim. Should be a valid jwt, which represents an external token (Microsoft Entra / MitId token when running in Azure)</param>
    /// <param name="roles">Values of the "role" claims. When running in Azure this could be something like "calculations:manage".</param>
    /// <param name="extraClaims">Extra claims to add to the internal token.</param>
    /// <returns>The internal token which wraps the provided external token</returns>
    public string CreateInternalToken(string externalToken, string[]? roles = null, Claim[]? extraClaims = null)
    {
        roles ??= [];
        extraClaims ??= [];

        var claims = new List<Claim>
        {
            new(TokenClaim, externalToken),
            new(JwtRegisteredClaimNames.Sub, SubValue),
            new(JwtRegisteredClaimNames.Azp, AzpValue),
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
}
