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
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ExampleHost.WebApi.Tests.Fixtures;

/// <summary>
/// A Http server that mocks "token configuration" endpoints as well as
/// expose an endpoint for creating access token's that can be used for
/// testing DH3 applications who require Http authentication and authorization.
/// </summary>
public class TokenMockServer : IDisposable
{
    private const string Kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
    private const string Issuer = "https://test.datahub.dk";

    private const string TokenClaim = "token";
    private const string RoleClaim = "role";

    // Path's used to configure endpoints in WireMock.NET
    // They must begin with "/".
    private const string ConfigurationEndpointPath = "/v2.0/.well-known/openid-configuration";
    private const string PublicKeysEndpointPath = "/discovery/v2.0/keys";

    private readonly RsaSecurityKey _testKey = new(RSA.Create()) { KeyId = Kid };
    private readonly WireMockServer _mockServer;

    /// <summary>
    /// Create the OpenId token server and preparing it for running with the specified port.
    /// OpenId configuration endpoints must use HTTPS. For this developers will need a developer certificate
    /// on their local machine and any build agent will need one locally as well.
    /// See WireMock.Net documentation for how to create and enable a developer certificate: https://github.com/WireMock-Net/WireMock.Net/wiki/Using-HTTPS-(SSL)
    /// </summary>
    /// <param name="port">Uses a default port, but can be specified to use another.</param>
    public TokenMockServer(int port = 1051)
    {
        // UNDONE: Instead of starting the server directly in the constructor, then maybe move to a method that we call to start end configure mock server?
        _mockServer = WireMockServer.Start(
            port: port,
            useSSL: true);
        MockTokenConfigurationEndpoints();
    }

    public string Url => _mockServer.Url!;

    public string MetadataAddress => $"{Url}{ConfigurationEndpointPath}";

    /// <summary>
    /// Creates an internal JWT containing an external JWT in the "token" claim,
    /// and a "role" claim per role specified in <paramref name="roles"/>.
    /// </summary>
    /// <param name="externalTokenString">External JWT as string.</param>
    /// <param name="roles">Commaseparated list of roles. Can be null or empty.</param>
    /// <returns>An internal JWT.</returns>
    public string GetToken(string externalTokenString, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(TokenClaim, externalTokenString),
            new(JwtRegisteredClaimNames.Sub, "A1AAB954-136A-444A-94BD-E4B615CA4A78"),
            new(JwtRegisteredClaimNames.Azp, "A1DEA55A-3507-4777-8CF3-F425A6EC2094"),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(RoleClaim, role.Trim()));
        }

        var externalJwt = new JwtSecurityToken(externalTokenString);
        var internalJwt = new JwtSecurityToken(
            issuer: Issuer,
            audience: externalJwt.Audiences.Single(),
            claims: claims,
            notBefore: externalJwt.ValidFrom,
            expires: externalJwt.ValidTo,
            signingCredentials: new SigningCredentials(_testKey, SecurityAlgorithms.RsaSha256));

        var internalToken = new JwtSecurityTokenHandler().WriteToken(internalJwt);
        return internalToken;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _mockServer.Dispose();
    }

    private void MockTokenConfigurationEndpoints()
    {
        MockGetConfiguration();
        MockGetPublicKeys();
    }

    private void MockGetConfiguration()
    {
        var request = Request
            .Create()
            .WithPath(ConfigurationEndpointPath)
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(JsonSerializer.Serialize(new
            {
                issuer = Issuer,
                jwks_uri = $"{Url}{PublicKeysEndpointPath}",
            }));

        _mockServer
            .Given(request)
            .RespondWith(response);
    }

    private void MockGetPublicKeys()
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_testKey);

        var request = Request
            .Create()
            .WithPath(PublicKeysEndpointPath)
            .UsingGet();

        var response = Response
            .Create()
            .WithStatusCode(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, "application/json")
            .WithBody(JsonSerializer.Serialize(new
            {
                keys = new[]
                {
                    new
                    {
                        kid = jwk.Kid,
                        kty = jwk.Kty,
                        n = jwk.N,
                        e = jwk.E,
                    },
                },
            }));

        _mockServer
            .Given(request)
            .RespondWith(response);
    }
}
