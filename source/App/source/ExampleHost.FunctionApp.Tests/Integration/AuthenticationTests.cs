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
using ExampleHost.FunctionApp.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

/// <summary>
/// Authentication tests using a nested token (a token which contains both an
/// external and an internal token) to verify that tokens are configured
/// to be validated as expected.
///
/// Similar tests exists for Web App in the 'AuthenticationTests' class
/// located in the 'ExampleHost.WebApi.Tests' project.
/// </summary>
[Collection(nameof(ExampleHostsCollectionFixture))]
public class AuthenticationTests : IAsyncLifetime
{
    public AuthenticationTests(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
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
    public async Task CallingInvalidEndpoint_WithNoToken_NotFound()
    {
        // Arrange

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication/does_not_exists");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CallingGetAnonymous_WithNoToken_Allowed()
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

    [Fact]
    public async Task CallingGetWithPermission_WithNoToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/auth/{requestIdentification}");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingGetWithPermission_WithFakeToken_Unauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/auth/{requestIdentification}");
        request.Headers.Authorization = Fixture.OpenIdJwtManager.JwtProvider.CreateFakeTokenAuthenticationHeader();
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingGetWithPermission_WithToken_Allowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authentication/auth/{requestIdentification}");
        request.Headers.Authorization = await Fixture.OpenIdJwtManager.JwtProvider.CreateInternalTokenAuthenticationHeaderAsync();
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingGetUserWithPermission_UserWithNoToken_Unauthorized()
    {
        // Arrange

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication/user");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CallingGetUserWithPermission_UserWithToken_ReturnsUserId()
    {
        // Arrange

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/authentication/user");
        request.Headers.Authorization = await Fixture.OpenIdJwtManager.JwtProvider.CreateInternalTokenAuthenticationHeaderAsync();
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        Guid.Parse(content).Should().NotBeEmpty();
    }
}
