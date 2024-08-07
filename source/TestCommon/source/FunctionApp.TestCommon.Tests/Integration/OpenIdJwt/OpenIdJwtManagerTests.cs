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
using Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.OpenIdJwt;

public class OpenIdJwtManagerTests
{
    [Fact]
    public async Task When_RunningOpenIdServer_Then_CanCallOpenIdConfigurationEndpoint()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager();
        openIdJwtManager.OpenIdServer.StartServer();

        // Act
        var httpClient = new HttpClient();

        var configurationResult = await httpClient.GetStringAsync($"{openIdJwtManager.OpenIdServer.MetadataAddress}");

        // Assert
        configurationResult.Should().NotBeNull();

        var jsonResult = JToken.Parse(configurationResult);
        jsonResult["issuer"].Should().NotBeNull();
        jsonResult["jwks_uri"].Should().NotBeNull();
    }

    [Fact]
    public async Task When_RunningOpenIdServer_Then_CanCallOpenIdPublicKeysEndpoint()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager();
        openIdJwtManager.OpenIdServer.StartServer();

        // Act
        var httpClient = new HttpClient();

        var configurationResult = await httpClient.GetStringAsync($"{openIdJwtManager.OpenIdServer.PublicKeysAddress}");

        // Assert
        configurationResult.Should().NotBeNullOrWhiteSpace();

        var jsonResult = JToken.Parse(configurationResult);
        jsonResult["keys"].Should().NotBeNull();
    }

    [Fact]
    public void Given_ExternalTokenAndRoles_When_CreatingInternalToken_Then_CanParseInternalTokenWithExpectedValues()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager();

        var expectedIssuer = "https://test.datahub.dk";
        var expectedAudience = "test-common";
        var expectedRole1 = "role1";
        var expectedRole2 = "role2";
        var expectedSubject = "A1AAB954-136A-444A-94BD-E4B615CA4A78";
        var expectedAzp = "A1DEA55A-3507-4777-8CF3-F425A6EC2094";

        var expectedExternalToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "https://mitid.dk",
            audience: expectedAudience,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1)));

        // Act
        var internalToken = openIdJwtManager.JwtProvider.CreateInternalToken(expectedExternalToken, expectedRole1, expectedRole2);

        // Assert
        internalToken.Should().NotBeNullOrEmpty();

        var parsedToken = new JwtSecurityTokenHandler().ReadToken(internalToken) as JwtSecurityToken;

        parsedToken.Should().NotBeNull();
        parsedToken!.Issuer.Should().Be(expectedIssuer);
        parsedToken.Audiences.Should().Equal(expectedAudience);
        parsedToken.Subject.Should().Be(expectedSubject);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == "token" && c.Value == expectedExternalToken);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == "role" && c.Value == expectedRole1);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == "role" && c.Value == expectedRole2);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == expectedSubject);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Azp && c.Value == expectedAzp);
    }

    [Fact]
    public void Given_ExternalToken_When_CreatingInternalToken_Then_CanParseExternalTokenWithExpectedValuesFromInternalToken()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager();

        var expectedAudience = "test-common";
        var expectedIssuer = "https://mitid.dk";
        var expectedValidFrom = new DateTime(2024, 08, 07, 13, 37, 00, DateTimeKind.Utc);
        var expectedValidTo = expectedValidFrom.AddHours(1);

        var expectedExternalToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: expectedIssuer,
            audience: expectedAudience,
            notBefore: expectedValidFrom,
            expires: expectedValidTo));

        // Act
        var internalToken = openIdJwtManager.JwtProvider.CreateInternalToken(expectedExternalToken);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var parsedToken = tokenHandler.ReadToken(internalToken) as JwtSecurityToken;

        var externalTokenClaim = parsedToken!.Claims.Single(c => c.Type == "token");
        var externalToken = tokenHandler.ReadToken(externalTokenClaim.Value) as JwtSecurityToken;

        externalToken!.Issuer.Should().Be(expectedIssuer);
        externalToken.Audiences.Should().Equal(expectedAudience);
        externalToken.ValidFrom.Should().Be(expectedValidFrom);
        externalToken.ValidTo.Should().Be(expectedValidTo);
    }
}
