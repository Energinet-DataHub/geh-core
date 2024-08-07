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

    private readonly RsaSecurityKey _testSecurityKey = new(RSA.Create()) { KeyId = Kid };

    /// <summary>
    /// Create manager to handle OpenId and JWT.
    /// </summary>
    /// <param name="openIdServerPort">The port to run the OpenId configuration server on. Defaults to 1051.</param>
    /// <param name="jwtIssuer">The issuer used by the OpenId configuration server and written to the JWT when creating an internal token. Defaults to https://test.datahub.dk</param>
    /// <param name="jwtSubject">The subject value written to the JWT when creating an internal token. Defaults to A1AAB954-136A-444A-94BD-E4B615CA4A78</param>
    /// <param name="jwtAzp">The azp value written to the JWT when creating an internal token. Defaults to A1DEA55A-3507-4777-8CF3-F425A6EC2094</param>
    public OpenIdJwtManager(
        int openIdServerPort = 1051,
        string jwtIssuer = "https://test.datahub.dk",
        string jwtSubject = "A1AAB954-136A-444A-94BD-E4B615CA4A78",
        string jwtAzp = "A1DEA55A-3507-4777-8CF3-F425A6EC2094")
    {
        JwtProvider = new JwtProvider(jwtIssuer, _testSecurityKey, jwtSubject, jwtAzp);
        OpenIdServer = new OpenIdMockServer(jwtIssuer, _testSecurityKey, openIdServerPort);
    }

    /// <summary>
    /// A JWT provider used for creating internal JWT tokens for testing DH3 applications that
    /// require authentication and authorization. The tokens can be used by applications using OpenId if the <see cref="OpenIdServer"/>
    /// is running.
    /// </summary>
    public JwtProvider JwtProvider { get; }

    /// <summary>
    /// An OpenId configuration server used for running an OpenId JWT server mock for testing DH3 applications that
    /// require OpenId configuration endpoints. Can be used in combination with <see cref="JwtProvider"/> to create JWT tokens
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
