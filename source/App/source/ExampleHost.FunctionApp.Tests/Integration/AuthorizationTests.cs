﻿// Copyright 2020 Energinet DataHub A/S
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
/// Authorization tests using a nested token (a token which contains both an
/// external and an internal token). Focus is on verifying the use of the Authorize
/// attribute with Roles.
///
/// Similar tests exists for Web App in the 'AuthorizationTests' class
/// located in the 'ExampleHost.WebApi.Tests' project.
/// </summary>
[Collection(nameof(ExampleHostsCollectionFixture))]
public class AuthorizationTests : IAsyncLifetime
{
    private const string PermissionOrganizationView = "organizations:view";
    private const string PermissionGridAreasManage = "grid-areas:manage";

    public AuthorizationTests(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
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

    [Theory]
    [InlineData(PermissionOrganizationView, HttpStatusCode.OK)]
    [InlineData("", HttpStatusCode.Forbidden)]
    [InlineData(PermissionGridAreasManage, HttpStatusCode.Forbidden)]
    public async Task CallingApi01AuthorizationGetOrganizationReadPermission_WithRole_IsExpectedStatusCode(
        string role,
        HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(role);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authorization/org/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(PermissionOrganizationView, HttpStatusCode.OK)]
    [InlineData(PermissionGridAreasManage, HttpStatusCode.OK)]
    [InlineData(PermissionGridAreasManage + "," + PermissionOrganizationView, HttpStatusCode.OK)]
    [InlineData("", HttpStatusCode.Forbidden)]
    public async Task CallingApi01AuthorizationGetOrganizationOrGridAreasPermission_WithRole_IsExpectedStatusCode(
        string role,
        HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(role);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authorization/org_or_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(PermissionGridAreasManage + "," + PermissionOrganizationView, HttpStatusCode.OK)]
    [InlineData("", HttpStatusCode.Forbidden)]
    [InlineData(PermissionOrganizationView, HttpStatusCode.Forbidden)]
    [InlineData(PermissionGridAreasManage, HttpStatusCode.Forbidden)]
    public async Task CallingApi01AuthorizationGetOrganizationAndGridAreasPermission_WithRole_IsExpectedStatusCode(
        string role,
        HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var requestIdentification = Guid.NewGuid().ToString();
        var authenticationHeader = await Fixture.CreateAuthenticationHeaderWithNestedTokenAsync(role);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/authorization/org_and_grid/{requestIdentification}");
        request.Headers.Add("Authorization", authenticationHeader);
        using var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

        // Assert
        actualResponse.StatusCode.Should().Be(expectedStatusCode);
    }
}
