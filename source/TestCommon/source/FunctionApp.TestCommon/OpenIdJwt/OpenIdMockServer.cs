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

using System.Net;
using System.Text.Json;
using Energinet.DataHub.Core.FunctionApp.TestCommon.TestCertificate;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;

/// <summary>
/// An OpenId configuration server used for running an OpenId JWT server mock for testing DH3 applications that
/// require OpenId configuration endpoints. Use in combination with <see cref="JwtProvider"/> to create JWT tokens
/// that can be validated according to the OpenId configuration provided by this server.
/// </summary>
public sealed class OpenIdMockServer : IDisposable, IOpenIdServer
{
    // Path's used to configure endpoints in WireMock.NET
    // They must begin with "/".
    private const string ConfigurationEndpointPath = "/v2.0/.well-known/openid-configuration";
    private const string PublicKeysEndpointPath = "/discovery/v2.0/keys";

    private readonly int _port;

    private WireMockServer? _mockServer;

    internal OpenIdMockServer(string issuer, RsaSecurityKey securityKey, int port)
    {
        Issuer = issuer;
        SecurityKey = securityKey;
        _port = port;
    }

    /// <summary>
    /// Get the URL of the running OpenId server.
    /// <remarks>
    /// ATTENTION: This requires the server to already be running or an exception will be thrown.
    /// Ensure that the <see cref="StartServer"/> method has been executed beforehand.
    /// </remarks>
    /// </summary>
    public string Url => GetRunningServer().Url!;

    /// <summary>
    /// Get the address of the running server's OpenId configuration endpoint.
    /// <remarks>
    /// ATTENTION: This requires the server to already be running or an exception will be thrown.
    /// Ensure that the <see cref="StartServer"/> method has been executed beforehand.
    /// </remarks>
    /// </summary>
    public string MetadataAddress => $"{Url}{ConfigurationEndpointPath}";

    internal string Issuer { get; }

    internal RsaSecurityKey SecurityKey { get; }

    public void StartServer()
    {
        _mockServer = WireMockServer.Start(new WireMockServerSettings
        {
            Port = _port,
            UseSSL = true,
            CertificateSettings = new WireMockCertificateSettings
            {
                X509CertificateFilePath = TestCertificateManager.FilePath,
                X509CertificatePassword = TestCertificateManager.Password,
            },
        });

        MockTokenConfigurationEndpoints();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mockServer?.Dispose();
            _mockServer = null;
        }
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
                jwks_uri = $"{GetRunningServer().Url}{PublicKeysEndpointPath}",
            }));

        GetRunningServer()
            .Given(request)
            .RespondWith(response);
    }

    private void MockGetPublicKeys()
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(SecurityKey);

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

        GetRunningServer()
            .Given(request)
            .RespondWith(response);
    }

    private WireMockServer GetRunningServer()
    {
        return _mockServer ?? throw new InvalidOperationException($"Cannot get running server. Make sure the server is running by calling {nameof(StartServer)}() first.");
    }
}
