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
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using ExampleHost.FunctionApp.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

/// <summary>
/// Subsystem Authentication tests using the ExampleHost.FunctionApp01 to call
/// the ExampleHost.FunctionApp02 over http using a standard JWT with the
/// expected "scope" as configured by <see cref="SubsystemAuthenticationOptions"/>.
/// </summary>
[Collection(nameof(ExampleHostsCollectionFixture))]
public class SubsystemAuthenticationTests : IAsyncLifetime
{
    public SubsystemAuthenticationTests(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
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

    /// <summary>
    /// This test calls ExampleHost.FunctionApp02 directly without a token.
    /// </summary>
    [Fact]
    public async Task Given_NoToken_When_GetAnonymous_Then_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/subsystemauthentication/anonymous/{requestIdentification}");
        using var actualResponse = await Fixture.App02HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// This test calls ExampleHost.FunctionApp02 directly without a token.
    /// </summary>
    [Fact]
    public async Task Given_NoToken_When_GetWithPermission_Then_IsUnauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/subsystemauthentication/authentication/{requestIdentification}");
        using var actualResponse = await Fixture.App02HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// This test uses the ExampleHost.FunctionApp01 to call the ExampleHost.FunctionApp02.
    /// In ExampleHost.FunctionApp01 we have configured a http client to use a standard JWT
    /// with the expected "scope" as configured by <see cref="SubsystemAuthenticationOptions"/>.
    /// By using this http client we should be able to call the protected endpoint in App02.
    /// </summary>
    [Fact]
    public async Task Given_ValidToken_When_GetWithPermission_Then_IsAllowed()
    {
        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/subsystemauthentication/authentication");
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
