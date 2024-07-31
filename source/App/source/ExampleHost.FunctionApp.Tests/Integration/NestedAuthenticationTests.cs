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
using ExampleHost.FunctionApp.Tests.Fixtures;
using ExampleHost.FunctionApp01.Functions;
using FluentAssertions;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

/// <summary>
/// Authentication tests using a nested token (a token which contains both an
/// external and an internal token).
///
/// Similar tests exists for Web App in the 'NestedAuthenticationTests' class
/// located in the 'ExampleHost.WebApi.Tests' project.
/// </summary>
[Collection(nameof(ExampleHostsCollectionFixture))]
public class NestedAuthenticationTests : IAsyncLifetime
{
    public NestedAuthenticationTests(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.SetTestOutputHelper(testOutputHelper);

        Fixture.App01HostManager.ClearHostLog();
    }

    private ExampleHostsFixture Fixture { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Fixture.SetTestOutputHelper(null!);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task CallingApi01InvalidEndpoint_WithNoToken_NotFound()
    {
        // Arrange

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication/does_not_exists");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CallingApi01AuthenticationGetAnonymous_WithNoToken_Succeeds()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/anon/{requestIdentification}");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    // TODO: Should return "Unauthorized" because of Authorize attribute. Currently this is returning "Unauthorized" because the UserMiddleware currently always expects a token.
    [Fact]
    public async Task CallingApi01AuthenticationGetWithPermission_WithNoToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/auth/{requestIdentification}");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // TODO: Should return "Unauthorized" because of Authorize attribute. Currently this is returning "OK" because the UserMiddleware currently expects a token with certain claims but doesn't otherwise validate the token.
    [Fact]
    public async Task CallingApi01AuthenticationGetWithPermission_WithFakeToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = CreateAuthenticationHeaderWithFakeToken();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // TODO: Should return "OK" because of Authorize attribute. Currently this is returning "OK" because the UserMiddleware currently expects a token with certain claims but doesn't otherwise validate the token.
    [Fact]
    public async Task CallingApi01AuthenticationGetWithPermission_WithToken_Succeeds()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationResult = await Fixture.GetTokenAsync();
        var authenticationHeader = await CreateAuthenticationHeaderWithNestedTokenAsync(authenticationResult);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/auth/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi01AuthenticationGetUserWithPermission_UserWithNoToken_Unauthorized()
    {
        // Arrange

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication/user");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingApi01AuthenticationGetUserWithPermission_UserWithToken_ReturnsUserId()
    {
        // Arrange
        var authenticationResult = await Fixture.GetTokenAsync();
        var authenticationHeader = await CreateAuthenticationHeaderWithNestedTokenAsync(authenticationResult);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication/user");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        Guid.Parse(content).Should().NotBeEmpty();
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

    /// <summary>
    /// Calls the <see cref="MockedTokenFunction"/> to create an "internal token"
    /// and returns a 'Bearer' authentication header.
    /// </summary>
    private async Task<string> CreateAuthenticationHeaderWithNestedTokenAsync(AuthenticationResult externalAuthenticationResult)
    {
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "api/token");
        tokenRequest.Content = new StringContent(externalAuthenticationResult.AccessToken);
        using var tokenResponse = await Fixture.App01HostManager.HttpClient.SendAsync(tokenRequest);

        var nestedToken = await tokenResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(nestedToken))
            throw new InvalidOperationException("Nested token was not created.");

        var authenticationHeader = $"Bearer {nestedToken}";
        return authenticationHeader;
    }
}
