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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks.ServiceLiveEndpointHealthCheck;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.App.Common.Tests
{
    [UnitTest]
    public class ServiceHealthCheckTests : IAsyncLifetime
    {
        private Uri _dependentServiceUri;
        private WireMockServer _serverMock;

        public Task InitializeAsync()
        {
            var httpLocalhostServer = "http://localhost:8080/DependentService";
            _dependentServiceUri = new Uri(httpLocalhostServer);
            _serverMock = WireMockServer.Start(_dependentServiceUri.Port);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _serverMock.Stop();
            _serverMock.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Should_return_health_status_when_dependent_service_is_ok()
        {
            // Arrange
            _serverMock
                .Given(Request.Create().UsingGet())
                .RespondWith(Response.Create().WithStatusCode(System.Net.HttpStatusCode.OK));

            var sut = new ServiceHealthCheck(_dependencyServiceUri, () => _serverMock.CreateClient());

            // Act
            var actualResponse = await sut.CheckHealthAsync(new HealthCheckContext(), default);

            // Assert
            actualResponse.Status.ToString().Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }

        [Fact]
        public async Task Should_return_unhealth_status_when_dependent_service_is_unavailable()
        {
            // Arrange
            _serverMock
                .Given(Request.Create().UsingGet())
                .RespondWith(Response.Create().WithStatusCode(System.Net.HttpStatusCode.ServiceUnavailable));

            var sut = new ServiceHealthCheck(_dependencyServiceUri, () => _serverMock.CreateClient());

            // Act
            var actualResponse = await sut.CheckHealthAsync(new HealthCheckContext(), default);

            // Assert
            actualResponse.Status.ToString().Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Unhealthy));
        }
    }
}
