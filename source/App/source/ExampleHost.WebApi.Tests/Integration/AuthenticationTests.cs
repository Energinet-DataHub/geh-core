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
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
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
    public async Task CallingInvalidEndpoint_WithNoToken_NotFound()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "webapi03/authentication/does_not_exist");

        // Act
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CallingGetAnonymous_WithNoToken_Allowed()
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
    public async Task CallingGetWithPermission_WithNoToken_Unauthorized()
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
    public async Task CallingGetWithPermission_WithFakeToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        request.Headers.Authorization = Fixture.OpenIdJwtManager.JwtProvider.CreateFakeTokenAuthenticationHeader();
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingGetWithPermission_WithToken_Allowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        request.Headers.Authorization = await Fixture.OpenIdJwtManager.JwtProvider.CreateInternalTokenAuthenticationHeaderAsync();
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingGetWithPermission_WithToken_ButUserIsDenied()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authentication/auth/{requestIdentification}");
        request.Headers.Authorization = await Fixture.OpenIdJwtManager.JwtProvider.CreateInternalTokenAuthenticationHeaderAsync();
        request.Headers.Add("DenyUser", string.Empty);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingGetUserWithPermission_UserWithToken_ReturnsUserId()
    {
        // Arrange

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "webapi03/authentication/user");
        request.Headers.Authorization = await Fixture.OpenIdJwtManager.JwtProvider.CreateInternalTokenAuthenticationHeaderAsync();
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        Assert.True(Guid.TryParse(content, out _));
    }
}
