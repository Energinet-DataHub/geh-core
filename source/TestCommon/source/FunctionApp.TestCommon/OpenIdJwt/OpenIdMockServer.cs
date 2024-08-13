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
using System.Security.Cryptography;
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
internal sealed class OpenIdMockServer : IDisposable
{
    // Path's used to configure endpoints in WireMock.NET
    // They must begin with "/".
    public const string ConfigurationEndpointPath = "/v2.0/.well-known/openid-configuration";

    private const string PublicKeysEndpointPath = "/discovery/v2.0/keys";

    private readonly int _port;

    private WireMockServer? _mockServer;

    /// <summary>
    /// Create an instance of the OpenIdMockServer that uses WireMock to run an OpenId configuration server.
    /// </summary>
    /// <param name="issuer">The issuer of the tokens</param>
    /// <param name="port">The port to run the server on</param>
    /// <param name="securityKey">An optional security key. If none is provided then a default key will be created.</param>
    internal OpenIdMockServer(string issuer, int port, RsaSecurityKey? securityKey = null)
    {
        Issuer = issuer;
        SecurityKey = securityKey ?? new(RSA.Create()) { KeyId = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F" };
        _port = port;
    }

    /// <summary>
    /// The full address of the running server's OpenId configuration metadata endpoint.
    /// </summary>
    public string MetadataAddress => $"{Url}{ConfigurationEndpointPath}";

    /// <summary>
    /// Whether the OpenId server is already running
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// The issuer which must be used to create JWT tokens that are valid according to this server's OpenId configuration
    /// </summary>
    internal string Issuer { get; }

    /// <summary>
    /// The security key which must be used to create JWT tokens that are valid according to this server's OpenId configuration
    /// </summary>
    internal RsaSecurityKey SecurityKey { get; }

    /// <summary>
    /// The base URL of the OpenId server.
    /// </summary>
    private string Url => $"https://localhost:{_port}";

    /// <summary>
    /// Start the OpenId server using WireMock. A test certificate will be installed to support HTTPS.
    /// If the server is already running then an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    public void StartServer()
    {
        if (IsRunning)
            throw new InvalidOperationException("Cannot start server since the OpenId server is already running.");

        TestCertificateProvider.InstallCertificate();

        _mockServer = WireMockServer.Start(new WireMockServerSettings
        {
            Port = _port,
            UseSSL = true,
            CertificateSettings = new WireMockCertificateSettings
            {
                X509CertificateFilePath = TestCertificateProvider.FilePath,
                X509CertificatePassword = TestCertificateProvider.Password,
            },
        });

        MockTokenConfigurationEndpoints();

        IsRunning = true;
    }

    public void Dispose()
    {
        _mockServer?.Dispose();
        _mockServer = null;
        IsRunning = false;
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
