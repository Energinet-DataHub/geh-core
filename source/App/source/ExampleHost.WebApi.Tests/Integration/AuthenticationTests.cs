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
using Energinet.DataHub.Core.App.Common.Security;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Authentication tests ensuring that the configured token is validated correctly.
/// </summary>
[Collection(nameof(AuthenticationHostCollectionFixture))]
public sealed class AuthenticationTests
{
    public AuthenticationTests(AuthenticationHostFixture fixture)
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
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi04/auth/anon/{requestIdentification}");
        var actualResponse = await Fixture.Web04HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    // TODO: Write tests...
}
