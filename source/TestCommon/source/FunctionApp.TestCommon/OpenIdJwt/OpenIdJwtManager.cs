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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.TestCertificate;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;

/// <summary>
/// Used to help test DH3 applications that requires OpenId and JWT for HTTP authentication and authorization.
/// The OpenIdJwtManager supports:
/// <list type="bullet">
///     <item>
///         <description>Starting an OpenId JWT server mock used for running tests that require OpenId configuration endpoints.</description>
///     </item>
///     <item>
///         <description>Creating internal JWT's used for testing DH3 applications that require authentication and authorization.</description>
///     </item>
/// </list>
///
/// A test certificate will be automatically installed on startup to support https (using <see cref="TestCertificateProvider"/>.<see cref="TestCertificateProvider.InstallCertificate"/>)
/// </summary>
public class OpenIdJwtManager : IDisposable
{
    private const string Kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";

    private readonly RsaSecurityKey _testSecurityKey = new(RSA.Create()) { KeyId = Kid };

    /// <summary>
    /// Create manager to handle OpenId and JWT.
    /// </summary>
    /// <param name="azureB2CSettings">Azure B2C settings used to get an external token. Can be retrieved from <see cref="IntegrationTestConfiguration"/></param>
    /// <param name="openIdServerPort">The port to run the OpenId configuration server on.</param>
    /// <param name="jwtIssuer">The issuer used by the OpenId configuration server and written to the JWT when creating an internal token.</param>
    public OpenIdJwtManager(
        AzureB2CSettings azureB2CSettings,
        int openIdServerPort = 1051,
        string jwtIssuer = "https://test-common.datahub.dk")
    {
        OpenIdServer = new OpenIdMockServer(jwtIssuer, _testSecurityKey, openIdServerPort);
        JwtProvider = new JwtProvider(azureB2CSettings, OpenIdServer.Issuer, OpenIdServer.SecurityKey);
    }

    /// <summary>
    /// A JWT provider used for creating internal JWT's for testing DH3 applications that
    /// require authentication and authorization. The tokens can be used by applications using OpenId if the <see cref="OpenIdServer"/>
    /// is running.
    /// </summary>
    public JwtProvider JwtProvider { get; }

    /// <summary>
    /// Start the OpenId JWT server using WireMock. The server is running at port specified by the configuration.
    /// The server will be listening for requests on the following endpoints, which are defined in the OpenId specification:
    /// <list type="bullet">
    ///     <item>
    ///         <description>/v2.0/.well-known/openid-configuration</description>
    ///     </item>
    ///     <item>
    ///         <description>/discovery/v2.0/keys</description>
    ///     </item>
    /// </list>
    /// The OpenId configuration endpoints must use HTTPS, so a developer certificate is installed and used automatically.
    /// <remarks>If the server is already started then an exception will be thrown.</remarks>
    /// </summary>
    public void StartServer() => OpenIdServer.StartServer();

    /// <summary>
    /// The base URL of the OpenId server.
    /// </summary>
    public string Url => OpenIdServer.Url;

    /// <summary>
    /// The full URL of the OpenId server's configuration metadata endpoint which should be used to
    /// get the OpenId configuration required to verify the internal token.
    /// </summary>
    public string InternalMetadataAddress => OpenIdServer.MetadataAddress;

    /// <summary>
    /// The full URL of the configuration metadata endpoint which should be used to
    /// get the OpenId configuration required to verify the external token.
    /// </summary>
    public string ExternalMetadataAddress => $"{JwtProvider.ExternalTokenAuthorityUrl}/{OpenIdMockServer.ConfigurationEndpointPath}";

    /// <summary>
    /// An OpenId configuration server used for running an OpenId JWT server mock for testing DH3 applications that
    /// require OpenId configuration endpoints. Can be used in combination with <see cref="JwtProvider"/> to create JWT tokens
    /// that can be validated according to the OpenId configuration provided by this server.
    /// </summary>
    private OpenIdMockServer OpenIdServer { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            OpenIdServer.Dispose();
    }
}
