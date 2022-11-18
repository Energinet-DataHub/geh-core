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
using System.Text;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Authentication tests ensuring that the configured token is validated correctly.
/// </summary>
[Collection(nameof(NestedAuthenticationHostCollectionFixture))]
public sealed class NestedAuthenticationTests
{
    public NestedAuthenticationTests(NestedAuthenticationHostFixture fixture)
    {
        Fixture = fixture;
    }

    private AuthenticationHostFixture Fixture { get; }

    [Fact]
    public async Task CallingApi04Get_Anonymous_Succeeds()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/authentication/anon/{requestIdentification}");
        using var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

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
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/authentication/auth/{requestIdentification}");
        using var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingApi04Get_AuthWithToken_Allowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();
        var authenticationHeader = await CreateNestedTokenAsync(authenticationResult);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

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
        var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();
        var authenticationHeader = await CreateNestedTokenAsync(authenticationResult);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        request.Headers.Add("DenyUser", authenticationHeader);
        using var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingApi04Get_UserWithToken_ReturnsUserId()
    {
        // Arrange
        var authenticationResult = await Fixture.BackendAppAuthenticationClient.GetAuthenticationTokenAsync();
        var authenticationHeader = await CreateNestedTokenAsync(authenticationResult);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/authentication/user");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        Assert.True(Guid.TryParse(content, out _));
    }

    [Fact]
    public async Task CallingApi04Get_AuthWithFakeToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("not-a-secret-key"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var subClaim = new Claim("sub", Guid.NewGuid().ToString());

        var securityToken = new JwtSecurityToken(claims: new[] { subClaim }, signingCredentials: credentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(securityToken);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", $"Bearer {token}");
        using var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> CreateNestedTokenAsync(AuthenticationResult authenticationResult)
    {
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "webapi04/token");
        tokenRequest.Content = new StringContent(authenticationResult.AccessToken);
        using var tokenResponse = await Fixture.Web04HttpClient.SendAsync(tokenRequest);

        var authenticationHeader = $"Bearer {await tokenResponse.Content.ReadAsStringAsync()}";
        return authenticationHeader;
    }
}
