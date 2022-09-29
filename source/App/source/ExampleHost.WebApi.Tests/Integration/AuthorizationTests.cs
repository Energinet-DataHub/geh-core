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
/// Authorization tests ensuring that the configured permissions are working.
/// </summary>
[Collection(nameof(AuthorizationHostCollectionFixture))]
public sealed class AuthorizationTests
{
    public AuthorizationTests(AuthorizationHostFixture fixture)
    {
        Fixture = fixture;
    }

    private AuthorizationHostFixture Fixture { get; }

    [Fact]
    public async Task CallingApi03Get_Anonymous_Succeeds()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/anon/{requestIdentification}");
        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWithClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Accountant.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWithNoClaimInToken_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken());

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWithWrongClaimInToken_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Supporter.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationWriteWithClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:write/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Supporter.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWriteWithReadClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read+org:write/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Accountant.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWriteWithWriteClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read+org:write/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Supporter.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWriteWithBothClaimsInToken_OrClaims_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read+org:write/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Accountant.ToString(), UserRoles.Supporter.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWriteWithBothClaimsInToken_AndClaims_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read_and_org:write/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Accountant.ToString(), UserRoles.Supporter.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationReadWriteWithOnlyOneClaimsInToken_AndClaims_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org:read_and_org:write/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(UserRoles.Supporter.ToString()));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static string CreateBearerToken(params string[] claims)
    {
        var extensionClaim = string.Join(',', claims.Select(c => $"\"{c}\""));
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("not-a-secret-key"));

        var token = new JwtSecurityToken(
            claims: new[] { new Claim("extension_roles", $"[{extensionClaim}]") },
            signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256));

        var handler = new JwtSecurityTokenHandler();
        return $"Bearer {handler.WriteToken(token)}";
    }
}
