﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Security.Cryptography;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ExampleHost.FunctionApp01.Functions;

/// <summary>
/// A mocked token function used to test authentication middleware.
///
/// This function is called when we from tests:
///  * Retrieve an "internal token".
/// </summary>
public class MockedTokenFunction
{
    private const string Kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
    private const string Issuer = "https://test.datahub.dk";
    private const string TokenClaim = "token";

    private static readonly RsaSecurityKey _testKey = new(RSA.Create()) { KeyId = Kid };

    [Function(nameof(GetToken))]
    public async Task<string> GetToken(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "token")]
        HttpRequestData httpRequest)
    {
        using var externalTokenReader = new StreamReader(httpRequest.Body);
        var rawExternalToken = await externalTokenReader.ReadToEndAsync().ConfigureAwait(false);

        var tokenHandler = new JsonWebTokenHandler();
        var externalToken = (JsonWebToken)tokenHandler.ReadToken(rawExternalToken);

        var claims = new Dictionary<string, object>
        {
            [TokenClaim] = rawExternalToken,
            [JwtRegisteredClaimNames.Sub] = "A1AAB954-136A-444A-94BD-E4B615CA4A78",
            [JwtRegisteredClaimNames.Azp] = "A1DEA55A-3507-4777-8CF3-F425A6EC2094",
        };

        var internalToken = new SecurityTokenDescriptor()
        {
            Issuer = Issuer,
            Audience = externalToken.Audiences.Single(),
            Claims = claims,
            NotBefore = externalToken.ValidFrom,
            Expires = externalToken.ValidTo,
            SigningCredentials = new SigningCredentials(_testKey, SecurityAlgorithms.RsaSha256),
        };

        var writtenToken = tokenHandler.CreateToken(internalToken);

        return writtenToken;
    }
}
