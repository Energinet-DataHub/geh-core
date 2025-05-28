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
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Subsystem Authentication tests that verifies the subsystem authentication
/// configuration in ExampleHost.WebApi02 is working as expected with
/// the attributes '[AllowAnonymous]' and '[Authorize]'.
/// </summary>
/// <remarks>
/// Similar tests exists for Function App in the 'SubsystemAuthenticationTests' class
/// located in the 'ExampleHost.FunctionApp.Tests' project.
/// </remarks>
[Collection(nameof(ExampleHostCollectionFixture))]
public class SubsystemAuthenticationTests
{
    public SubsystemAuthenticationTests(ExampleHostFixture fixture)
    {
        Fixture = fixture;
    }

    private ExampleHostFixture Fixture { get; }

    /// <summary>
    /// This test calls an ExampleHost.WebApi02 endpoint directly without a token.
    /// The endpoint is marked with '[AllowAnonymous]'.
    /// </summary>
    [Fact]
    public async Task Given_NoToken_When_CallingApi02GetAnonymousForSubsystem_Then_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi02/subsystemauthentication/anonymous/{requestIdentification}");
        using var actualResponse = await Fixture.Web02HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// This test calls an ExampleHost.WebApi02 endpoint directly without a token.
    /// The endpoint is marked with '[Authorize]'.
    /// </summary>
    [Fact]
    public async Task Given_NoToken_When_CallingApi02GetWithPermissionForSubsystem_Then_IsUnauthorized()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi02/subsystemauthentication/authentication/{requestIdentification}");
        using var actualResponse = await Fixture.Web02HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// This test uses the ExampleHost.WebApi01 to call an ExampleHost.WebApi02
    /// endpoint that is marked with '[Authorize]'.
    /// In ExampleHost.WebApi01 we have configured a http client to use a standard JWT
    /// with the expected "scope" as configured by <see cref="SubsystemAuthenticationOptions"/>.
    /// By using this http client we should be able to call the protected endpoint in ExampleHost.WebApi02.
    /// </summary>
    [Fact]
    public async Task Given_ValidToken_When_CallingApi02GetWithPermissionForSubsystemThroughApi01_Then_IsAllowed()
    {
        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi01/subsystemauthentication/authentication");
        using var actualResponse = await Fixture.Web01HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
