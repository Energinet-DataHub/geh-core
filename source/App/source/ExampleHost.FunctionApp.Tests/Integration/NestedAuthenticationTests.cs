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
using ExampleHost.FunctionApp01.Functions;
using FluentAssertions;
using Microsoft.Identity.Client;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

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
    public async Task CallingApi01AuthenticationGet_UserWithToken_ReturnsUserId()
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
        var authenticationHeader = $"Bearer {nestedToken}";
        return authenticationHeader;
    }
}
