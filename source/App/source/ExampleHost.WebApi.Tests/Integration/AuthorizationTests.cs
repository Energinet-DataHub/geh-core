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

    [Theory]
    [InlineData(HttpStatusCode.OK, PermissionOrganizationView)]
    [InlineData(HttpStatusCode.Forbidden, "")]
    [InlineData(HttpStatusCode.Forbidden, PermissionGridAreasManage)]
    public async Task CallingGetOrganizationReadPermission_WithRole_IsExpectedStatusCode(
        HttpStatusCode expectedStatusCode,
        params string[] roles)
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(roles);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, PermissionOrganizationView)]
    [InlineData(HttpStatusCode.OK, PermissionGridAreasManage)]
    [InlineData(HttpStatusCode.OK, PermissionGridAreasManage, PermissionOrganizationView)]
    [InlineData(HttpStatusCode.Forbidden, "")]
    public async Task CallingGetOrganizationOrGridAreasPermission_WithRole_IsExpectedStatusCode(
        HttpStatusCode expectedStatusCode,
        params string[] roles)
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(roles);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, PermissionGridAreasManage, PermissionOrganizationView)]
    [InlineData(HttpStatusCode.Forbidden, "")]
    [InlineData(HttpStatusCode.Forbidden, PermissionOrganizationView)]
    [InlineData(HttpStatusCode.Forbidden, PermissionGridAreasManage)]
    public async Task CallingGetOrganizationAndGridAreasPermission_WithRole_IsExpectedStatusCode(
        HttpStatusCode expectedStatusCode,
        params string[] roles)
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(roles);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi03/authorization/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.Web03HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(expectedStatusCode);
    }
}
