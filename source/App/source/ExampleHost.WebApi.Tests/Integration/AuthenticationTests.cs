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
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Authentication tests using a nested token (a token which contains both an
/// external and an internal token) to verify that tokens are configured
/// to be validated as expected.
///
/// Similar tests exists for Function App in the 'AuthenticationTests' class
/// located in the 'ExampleHost.FunctionApp.Tests' project.
/// </summary>
[Collection(nameof(WebApi03HostCollectionFixture))]
public sealed class AuthenticationTests
{
    public AuthenticationTests(WebApi03HostFixture fixture)
    {
        Fixture = fixture;
    }

    private WebApi03HostFixture Fixture { get; }

    [Fact]
    public async Task CallingApi04Get_WithNoToken_NotFound()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "webapi03/authentication/does_not_exist");

        // Act
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CallingApi04GetAnonymous_WithNoToken_Succeeds()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/anon/{requestIdentification}");
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi04Get_AuthRequiredButNoToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingApi04Get_AuthWithFakeToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = CreateAuthenticationHeaderWithFakeToken();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingApi04Get_AuthWithToken_Allowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi04Get_AuthWithToken_ButUserIsDenied()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        request.Headers.Add("DenyUser", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingApi04Get_UserWithToken_ReturnsUserId()
    {
        // Arrange
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "webapi03/authentication/user");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        Assert.True(Guid.TryParse(content, out _));
    }

    private static string CreateAuthenticationHeaderWithFakeToken()
    {
        var securityKey = new SymmetricSecurityKey("not-a-secret-keynot-a-secret-key"u8.ToArray());
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userIdAsSubClaim = new Claim("sub", Guid.NewGuid().ToString());
        var actorIdAsAzpClaim = new Claim("azp", Guid.NewGuid().ToString());

        var securityToken = new JwtSecurityToken(claims: [userIdAsSubClaim, actorIdAsAzpClaim], signingCredentials: credentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(securityToken);

        var authenticationHeader = $"Bearer {token}";
        return authenticationHeader;
    }
}
