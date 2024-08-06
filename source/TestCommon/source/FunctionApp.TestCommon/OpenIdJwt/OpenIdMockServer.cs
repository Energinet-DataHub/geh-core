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
public sealed class OpenIdMockServer : IDisposable
{
    // Path's used to configure endpoints in WireMock.NET
    // They must begin with "/".
    private const string ConfigurationEndpointPath = "/v2.0/.well-known/openid-configuration";
    private const string PublicKeysEndpointPath = "/discovery/v2.0/keys";

    private readonly string _issuer;
    private readonly RsaSecurityKey _securityKey;
    private readonly int _port;

    private WireMockServer? _mockServer;

    internal OpenIdMockServer(string issuer, RsaSecurityKey securityKey, int port)
    {
        _issuer = issuer;
        _securityKey = securityKey;
        _port = port;
    }

    public string Url => GetRunningServer().Url!;

    public string MetadataAddress => $"{Url}{ConfigurationEndpointPath}";

    /// <summary>
    /// Start and the OpenId JWT server using WireMock. The server is running at port specified by the configuration (defaults to port 1051).
    /// OpenId configuration endpoints must use HTTPS, so a developer certificate is provided and used automatically.
    /// See WireMock.Net documentation for more information about developer certificates: https://github.com/WireMock-Net/WireMock.Net/wiki/Using-HTTPS-(SSL)
    ///
    /// The server will be listening for requests on the following endpoints, which are defined in the OpenId specification:
    /// - /v2.0/.well-known/openid-configuration
    /// - /discovery/v2.0/keys
    /// </summary>
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
                issuer = _issuer,
                jwks_uri = $"{GetRunningServer().Url}{PublicKeysEndpointPath}",
            }));

        GetRunningServer()
            .Given(request)
            .RespondWith(response);
    }

    private void MockGetPublicKeys()
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_securityKey);

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
        return _mockServer ?? throw new InvalidOperationException($"Server is not started. Call {nameof(StartServer)}() first.");
    }
}
