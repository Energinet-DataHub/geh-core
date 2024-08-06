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

using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;

/// <summary>
/// A Http server that mocks "JWT token configuration" endpoints as well as
/// expose an endpoint for creating access token's that can be used for
/// testing DH3 applications that require Http authentication and authorization.
/// </summary>
/// <summary>
/// Used to help test DH3 applications that requires OpenId and JWT for HTTP authentication and authorization.
/// The OpenIdJwtManager supports:
/// - Starting an OpenId JWT server mock used for running tests that require OpenId configuration endpoints
/// - Creating internal JWT tokens used for testing DH3 applications that require authentication and authorization
/// </summary>
public class OpenIdJwtManager : IDisposable
{
    private const string Kid = "049B6F7F-F5A5-4D2C-A407-C4CD170A759F";
    private const string Issuer = "https://test.datahub.dk";

    private readonly RsaSecurityKey _testSecurityKey = new(RSA.Create()) { KeyId = Kid };

    /// <summary>
    /// Create manager to handle OpenId and JWT.
    /// <param name="openIdServerPort">The port to run the OpenId JWT server on. Defaults to 1051.</param>
    /// </summary>
    public OpenIdJwtManager(int openIdServerPort = 1051)
    {
        JwtProvider = new JwtProvider(Issuer, _testSecurityKey);
        OpenIdServer = new OpenIdMockServer(Issuer, _testSecurityKey, openIdServerPort);
    }

    /// <summary>
    /// A JWT provider used for creating internal JWT tokens for testing DH3 applications that
    /// require authentication and authorization. The tokens can be used by applications using OpenId if the <see cref="OpenIdServer"/>
    /// is running.
    /// </summary>
    public JwtProvider JwtProvider { get; }

    /// <summary>
    /// An OpenId configuration server used for running an OpenId JWT server mock for testing DH3 applications that
    /// require OpenId configuration endpoints. Use in combination with the <see cref="JwtProvider"/> to create JWT tokens
    /// that can be validated according to the OpenId configuration provided by this server.
    /// </summary>
    public OpenIdMockServer OpenIdServer { get; }

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
