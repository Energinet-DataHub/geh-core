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
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Hosting.Tests.Fixtures;
using FluentAssertions;

namespace Energinet.DataHub.Core.App.Hosting.Tests.Diagnostics.HealthChecks
{
    public class HealthCheckEndpointRouteBuilderExtensionsTests
        : IClassFixture<HealthChecksFixture>
    {
        private readonly HealthChecksFixture _fixture;

        public HealthCheckEndpointRouteBuilderExtensionsTests(HealthChecksFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CallingLive_Should_ReturnOKAndHealthy()
        {
            // Act
            using var actualResponse = await _fixture.HttpClient.GetAsync("/monitor/live");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }

        [Fact]
        public async Task CallingReady_Should_ReturnOKAndHealthy()
        {
            // Act
            using var actualResponse = await _fixture.HttpClient.GetAsync("/monitor/live");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }
    }
}
