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
/// Authorization tests using a nested token (a token which contains both an
/// external and an internal token). Focus is on verifying the use of the Authorize
/// attribute with Roles.
///
/// Similar tests exists for Function App in the 'AuthorizationTests' class
/// located in the 'ExampleHost.FunctionApp.Tests' project.
/// </summary>
[Collection(nameof(WebApi03HostCollectionFixture))]
public sealed class AuthorizationTests
{
    private const string PermissionOrganizationView = "organizations:view";
    private const string PermissionGridAreasManage = "grid-areas:manage";

    public AuthorizationTests(WebApi03HostFixture fixture)
    {
        Fixture = fixture;
    }

    private WebApi03HostFixture Fixture { get; }

    [Fact]
    public async Task CallingApi03Get_Anonymous_Succeeds()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/anon/{requestIdentification}");
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
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionOrganizationView);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

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
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationWithWrongClaimInToken_IsForbidden()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionGridAreasManage);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CallingApi03Get_GridAreasWithClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionGridAreasManage);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

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
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionOrganizationView);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationOrGridAreasWithGridClaimInToken_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionGridAreasManage);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CallingApi03Get_OrganizationOrGridAreasWithBothClaimsInToken_OrClaims_IsAllowed()
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionGridAreasManage, PermissionOrganizationView);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

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
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionGridAreasManage, PermissionOrganizationView);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

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
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(PermissionGridAreasManage);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);

        var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
