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
using ExampleHost.FunctionApp.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration
{
    /// <summary>
    /// Tests verifying the configuration and behaviour of Health Checks.
    /// </summary>
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class ExampleHostsHealthChecksTests : IAsyncLifetime
    {
        public ExampleHostsHealthChecksTests(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();
            Fixture.App02HostManager.ClearHostLog();
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

        [Fact]
        public async Task CallingLive_Should_ReturnOKAndExpectedContent()
        {
            // Act
            using var actualResponse = await Fixture.App01HostManager.HttpClient.GetAsync("api/monitor/live");

            // Assert
            using var assertionScope = new AssertionScope();

            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            ////actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
            actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");

            var content = await actualResponse.Content.ReadAsStringAsync();
            ////content.Should().StartWith("{\"status\":\"Healthy\"");
            content.Should().Be("Healthy");
        }

        [Fact]
        public async Task CallingReady_Should_ReturnOKAndExpectedContent()
        {
            // Act
            using var actualResponse = await Fixture.App01HostManager.HttpClient.GetAsync("api/monitor/ready");

            // Assert
            using var assertionScope = new AssertionScope();

            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            ////actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
            actualResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/plain");

            var content = await actualResponse.Content.ReadAsStringAsync();
            ////content.Should().StartWith("{\"status\":\"Healthy\"");
            content.Should().Be("Healthy");
        }
    }
}
