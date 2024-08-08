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
using Azure.Identity;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.OpenIdJwt;

/// <summary>
/// This class fixture ensures we reuse the <see cref="AzureB2CSettings"/> which is retrieved
/// from the <see cref="IntegrationTestConfiguration"/>, since it takes a couple of seconds because
/// it's using <see cref="DefaultAzureCredential"/>
/// </summary>
public class OpenIdJwtManagerTests : IClassFixture<OpenIdJwtManagerFixture>
{
    private readonly RsaSecurityKey _incorrectSigningKey = new(RSA.Create()) { KeyId = "149B6F7F-F5A5-4D2C-A407-C4CD170A759F" };

    public OpenIdJwtManagerTests(OpenIdJwtManagerFixture fixture)
    {
        Fixture = fixture;
    }

    private OpenIdJwtManagerFixture Fixture { get; }

    [Fact]
    public async Task Given_CreatedInternalToken_When_GettingOpenIdConfigurationFromServer_Then_TokenCanBeValidated()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager(Fixture.AzureB2CSettings);

        var internalToken = await openIdJwtManager.JwtProvider.CreateInternalTokenAsync();

        // Act
        openIdJwtManager.StartServer();
        var openIdConfig = await GetOpenIdConfigFromServer(openIdJwtManager.InternalMetadataAddress);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = openIdConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = Fixture.AzureB2CSettings.TestBffAppId,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = openIdConfig.SigningKeys,
            ValidateLifetime = true,
        };

        // Assert
        var validateToken = () => new JwtSecurityTokenHandler().ValidateToken(internalToken, validationParameters, out _);
        validateToken.Should().NotThrow();
    }

    [Fact]
    public async Task Given_CreatedInternalToken_When_OpenIdConfigurationSigningKeyIsIncorrect_Then_TokenValidationFails()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager(Fixture.AzureB2CSettings);

        var internalToken = await openIdJwtManager.JwtProvider.CreateInternalTokenAsync();

        // Act
        openIdJwtManager.StartServer();
        var openIdConfig = await GetOpenIdConfigFromServer(openIdJwtManager.InternalMetadataAddress);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = openIdConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = Fixture.AzureB2CSettings.TestBffAppId,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = [_incorrectSigningKey],
            ValidateLifetime = true,
        };

        // Assert
        var validateToken = () => new JwtSecurityTokenHandler().ValidateToken(internalToken, validationParameters, out _);
        validateToken.Should().ThrowExactly<SecurityTokenSignatureKeyNotFoundException>();
    }

    [Fact]
    public async Task Given_CreatedInternalToken_When_OpenIdConfigurationIssuerIsIncorrect_Then_TokenValidationFails()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager(Fixture.AzureB2CSettings);

        var internalToken = await openIdJwtManager.JwtProvider.CreateInternalTokenAsync();

        // Act
        openIdJwtManager.StartServer();
        var openIdConfig = await GetOpenIdConfigFromServer(openIdJwtManager.InternalMetadataAddress);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "incorrect-issuer",
            ValidateAudience = true,
            ValidAudience = Fixture.AzureB2CSettings.TestBffAppId,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = openIdConfig.SigningKeys,
            ValidateLifetime = true,
        };

        // Assert
        var validateToken = () => new JwtSecurityTokenHandler().ValidateToken(internalToken, validationParameters, out _);
        validateToken.Should().ThrowExactly<SecurityTokenInvalidIssuerException>();
    }

    [Fact]
    public async Task Given_ExternalTokenAndRoles_When_CreatingInternalToken_Then_CanParseInternalTokenWithExpectedValues()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager(Fixture.AzureB2CSettings);

        var expectedIssuer = "https://test-common.datahub.dk";
        var expectedAudience = Fixture.AzureB2CSettings.TestBffAppId;
        var expectedRole1 = "role1";
        var expectedRole2 = "role2";
        var expectedClaim1 = new Claim("claim1", "value1");
        var expectedClaim2 = new Claim("claim2", "value2");
        var expectedSubject = "A1AAB954-136A-444A-94BD-E4B615CA4A78";
        var expectedAzp = "A1DEA55A-3507-4777-8CF3-F425A6EC2094";

        // Act
        var internalToken = await openIdJwtManager.JwtProvider.CreateInternalTokenAsync(
            roles: [expectedRole1, expectedRole2],
            extraClaims: [expectedClaim1, expectedClaim2]);

        // Assert
        internalToken.Should().NotBeNullOrEmpty();

        var parsedToken = new JwtSecurityTokenHandler().ReadToken(internalToken) as JwtSecurityToken;

        parsedToken.Should().NotBeNull();

        using var assertionScope = new AssertionScope();
        parsedToken!.Issuer.Should().Be(expectedIssuer);
        parsedToken.Audiences.Should().Equal(expectedAudience);
        parsedToken.Subject.Should().Be(expectedSubject);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == "token"); // An external token should exist in the 'token' claim
        parsedToken.Claims.Should().ContainSingle(c => c.Type == "role" && c.Value == expectedRole1);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == "role" && c.Value == expectedRole2);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == expectedClaim1.Type && c.Value == expectedClaim1.Value);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == expectedClaim2.Type && c.Value == expectedClaim2.Value);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == expectedSubject);
        parsedToken.Claims.Should().ContainSingle(c => c.Type == JwtRegisteredClaimNames.Azp && c.Value == expectedAzp);
    }

    [Fact]
    public async Task Given_ExternalToken_When_CreatingInternalToken_Then_CanParseExternalTokenWithExpectedValuesFromInternalToken()
    {
        // Arrange
        using var openIdJwtManager = new OpenIdJwtManager(Fixture.AzureB2CSettings);

        var expectedAudience = Fixture.AzureB2CSettings.TestBffAppId;
        var expectedIssuer = "https://login.microsoftonline.com/72996b41-f6a7-44db-b070-65acc2fb7818/v2.0";

        // Act
        var internalToken = await openIdJwtManager.JwtProvider.CreateInternalTokenAsync();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var parsedToken = tokenHandler.ReadToken(internalToken) as JwtSecurityToken;

        var externalTokenClaim = parsedToken!.Claims.Single(c => c.Type == "token");
        var externalToken = tokenHandler.ReadToken(externalTokenClaim.Value) as JwtSecurityToken;

        using var assertionScope = new AssertionScope();
        externalToken!.Issuer.Should().Be(expectedIssuer);
        externalToken.Audiences.Should().Equal(expectedAudience);
    }

    private Task<OpenIdConnectConfiguration> GetOpenIdConfigFromServer(string metadataAddress)
    {
        var openIdConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever());

        return openIdConfigManager.GetConfigurationAsync(CancellationToken.None);
    }
}
