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
using System.Text.RegularExpressions;
using Energinet.DataHub.Core.App.WebApp.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Energinet.DataHub.Core.App.WebApp.Tests.Extensions.DependencyInjection;

public class OpenApiExtensionsTests : IClassFixture<OpenApiFixture>
{
    private readonly OpenApiFixture _fixture;

    public OpenApiExtensionsTests(OpenApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UrlIsApiVersionSwaggerJson_WhenGet_ResponseIsOKAndContainsJsonAndOpenAPIv3()
    {
        // Arrange
        var apiVersion = "v1";
        var majorVersion = int.Parse(Regex.Replace(apiVersion, "[a-zA-Z]", string.Empty));

        var url = $"swagger/{apiVersion}/swagger.json";
        var client = _fixture.GetClientWithApiVersion(majorVersion);

        // Act
        var actualResponse = await client.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain($"\"openapi\": \"3.");
    }

    [Fact]
    public async Task UrlVersionIsGreaterThanApiVersion_WhenGet_ResponseIsNotFound()
    {
        // Arrange
        var apiVersion = "v2";
        var majorVersion = int.Parse(Regex.Replace(apiVersion, "[a-zA-Z]", string.Empty));

        var url = $"swagger/{apiVersion}/swagger.json";
        var client = _fixture.GetClientWithApiVersion(majorVersion - 1);

        // Act
        var actualResponse = await client.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UrlVersionIsLessThanApiVersion_WhenGet_ResponseIsNotFound()
    {
        // Arrange
        var apiVersion = "v1";
        var majorVersion = int.Parse(Regex.Replace(apiVersion, "[a-zA-Z]", string.Empty));

        var url = $"swagger/{apiVersion}/swagger.json";
        var client = _fixture.GetClientWithApiVersion(majorVersion + 1);

        // Act
        var actualResponse = await client.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // This should test, that the version and title is configurable.
    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    [InlineData("v3")]
    public async Task UrlIsApiVersionSwaggerJson_WhenGet_ResponseIsOKAndContainsJsonAndCorrespondingVersion(string apiVersion)
    {
        // Arrange
        var url = $"swagger/{apiVersion}/swagger.json";
        var majorVersion = int.Parse(Regex.Replace(apiVersion, "[a-zA-Z]", string.Empty));
        var client = _fixture.GetClientWithApiVersion(majorVersion);

        // Act
        var actualResponse = await client.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain($"\"version\": \"{majorVersion}.");
    }

    // TODO: This should return ok
    [Fact]
    public async Task UrlIsSwaggerUIDefault_WhenGet_ResponseIsMovedPermanently()
    {
        // Arrange
        var url = "swagger";

        // Act
        var actualResponse = await _fixture.HttpClient.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.MovedPermanently);
    }

    [Fact]
    public async Task UrlIsSwaggerIndexHtmlUIDefault_WhenGet_ResponseIsOKAndContainsHtml()
    {
        // Arrange
        var url = "swagger/index.html";

        // Act
        var actualResponse = await _fixture.HttpClient.GetAsync(url);

        // Assert
        using var assertionScope = new AssertionScope();
        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }
}
