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
    public async Task CallingApi03Get_OrganizationWithClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken());

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationWithNoClaimInToken_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken());

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationWithWrongClaimInToken_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.GridAreas));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CallingApi03Get_GridAreasWithClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/grid/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.GridAreas));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationOrGridAreasWithOrgClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.Organization));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationOrGridAreasWithGridClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.GridAreas));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationOrGridAreasWithBothClaimsInToken_OrClaims_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.GridAreas, Permission.Organization));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationAndGridAreasWithBothClaimsInToken_AndClaims_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.GridAreas, Permission.Organization));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be(requestIdentification);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationAndGridAreasWithOnlyOneClaimsInToken_AndClaims_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/permission/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", CreateBearerToken(Permission.GridAreas));

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static string CreateBearerToken(params Permission[] permissions)
    {
        var extensionClaim = string.Join(',', permissions.Select(p => $"\"{PermissionsAsClaims.Lookup[p]}\""));
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("not-a-secret-key"));

        var token = new JwtSecurityToken(
            claims: new[] { new Claim("roles", $"[{extensionClaim}]") },
            signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256));

        var handler = new JwtSecurityTokenHandler();
        return $"Bearer {handler.WriteToken(token)}";
    }
}
