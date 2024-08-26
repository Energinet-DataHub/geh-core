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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Tests verifying the configuration and behaviour of Health Checks.
/// </summary>
[Collection(nameof(ExampleHostCollectionFixture))]
public class HealthChecksTests
{
    public HealthChecksTests(ExampleHostFixture fixture)
    {
        Fixture = fixture;
    }

    private ExampleHostFixture Fixture { get; }

    /// <summary>
    /// Verify the response contains JSON in a format that the Health Checks UI supports.
    /// </summary>
    [Theory]
    [InlineData(HealthChecksConstants.LiveHealthCheckEndpointRoute)]
    [InlineData(HealthChecksConstants.ReadyHealthCheckEndpointRoute)]
    [InlineData(HealthChecksConstants.StatusHealthCheckEndpointRoute)]
    public async Task CallingHealthCheck_Should_ReturnOKAndExpectedContent(string healthCheckEndpoint)
    {
        // Act
        using var actualResponse = await Fixture.Web01HttpClient.GetAsync(healthCheckEndpoint);

        // Assert
        using var assertionScope = new AssertionScope();

        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().StartWith("{\"status\":\"Healthy\"");
    }

    [Fact]
    public async Task CallingReadyEndpoint_Should_ReturnOKAndOnlyContainExpectedHealthChecks()
    {
        // Act
        using var actualResponse = await Fixture.Web01HttpClient.GetAsync(HealthChecksConstants.ReadyHealthCheckEndpointRoute);

        // Assert
        using var assertionScope = new AssertionScope();

        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain("verify-ready");
        content.Should().NotContain("verify-status");
    }

    [Fact]
    public async Task CallingStatusEndpoint_Should_ReturnOKAndOnlyContainExpectedHealthChecks()
    {
        // Act
        using var actualResponse = await Fixture.Web01HttpClient.GetAsync(HealthChecksConstants.StatusHealthCheckEndpointRoute);

        // Assert
        using var assertionScope = new AssertionScope();

        actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await actualResponse.Content.ReadAsStringAsync();
        content.Should().Contain("verify-status");
        content.Should().NotContain("verify-ready");
    }
}
