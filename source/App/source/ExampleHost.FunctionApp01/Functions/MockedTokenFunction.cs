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
using System.Security.Cryptography;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.IdentityModel.Tokens;

namespace ExampleHost.FunctionApp01.Functions;

public class MockedTokenFunction
{
    private const string Kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
    private const string TokenClaim = "token";
    private const string Issuer = "https://test.datahub.dk";

    private static readonly RsaSecurityKey _testKey = new(RSA.Create()) { KeyId = Kid };

    [Function(nameof(GetToken))]
    public async Task<string> GetToken(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "token")]
        HttpRequestData httpRequest)
    {
        using var body = new StreamReader(httpRequest.Body);
        var rawExternalToken = await body.ReadToEndAsync().ConfigureAwait(false);

        var externalToken = new JwtSecurityToken(rawExternalToken);
        var tokenClaim = new Claim(TokenClaim, rawExternalToken);

        var userClaim = new Claim(JwtRegisteredClaimNames.Sub, "A1AAB954-136A-444A-94BD-E4B615CA4A78");
        var actorClaim = new Claim(JwtRegisteredClaimNames.Azp, "A1DEA55A-3507-4777-8CF3-F425A6EC2094");

        var token = new JwtSecurityToken(
            Issuer,
            externalToken.Audiences.Single(),
            new[] { tokenClaim, userClaim, actorClaim },
            externalToken.ValidFrom,
            externalToken.ValidTo,
            new SigningCredentials(_testKey, SecurityAlgorithms.RsaSha256));

        var handler = new JwtSecurityTokenHandler();
        var writtenToken = handler.WriteToken(token);

        return writtenToken;
    }
}
