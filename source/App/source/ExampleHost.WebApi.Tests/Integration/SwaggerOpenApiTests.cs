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
using System.Text.RegularExpressions;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Tests verifying the configuration and behaviour of swagger and api versioning.
/// </summary>
[Collection(nameof(ExampleHostCollectionFixture))]
public class SwaggerOpenApiTests
{
    public SwaggerOpenApiTests(ExampleHostFixture fixture)
    {
        Fixture = fixture;
    }

    private ExampleHostFixture Fixture { get; }

    [Theory]
    [InlineData("SwaggerDisplay", "version 2")]
    [InlineData("SwaggerDisplay?api-version=2", "version 2")]
    [InlineData("SwaggerDisplay?api-version=1", "version 1")]
    public async Task UrlWithApiVersion_WhenGet_ResponseIsOKAndResponseContainsExpectedVersion(string url, string expectedVersion)
    {
        // Act
        var actualResponse = await Fixture.Web01HttpClient.GetAsync($"webapi01/{url}");

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Be($"Hello from swagger controller {expectedVersion} in WebApi01");
    }

    [Theory]
    [InlineData("v1", "ExampleHost.WebApi")]
    [InlineData("v2", "ExampleHost.WebApi")]
    public async Task UrlIsApiVersionSwaggerJson_WhenGet_ResponseIsOKAndContainsJsonAndCorrespondingVersion(string apiVersion, string title)
    {
        // Arrange
        var majorVersion = int.Parse(Regex.Replace(apiVersion, "[a-zA-Z]", string.Empty));
        var url = $"swagger/{apiVersion}/swagger.json";

        // Act
        var actualResponse = await Fixture.Web01HttpClient.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain($"\"info\": {{\n    \"title\": \"{title}\",\n ");
        content.Should().Contain($"\"version\": \"{majorVersion}.");
    }

    [Fact]
    public async Task UrlIsSwaggerIndexHtml_WhenGet_ResponseIsOKAndContainsHtml()
    {
        // Arrange
        var url = "swagger/index.html";

        // Act
        var actualResponse = await Fixture.Web01HttpClient.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }
}
