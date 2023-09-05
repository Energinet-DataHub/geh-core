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
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.App.Hosting.Tests.Diagnostics.HealthChecks
{
    public class HealthCheckEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public async Task CallingLive_Should_ReturnOKAndHealthy()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();

                services.AddHealthChecks()
                    .AddLiveCheck();
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapLiveHealthChecks();
                    endpoints.MapReadyHealthChecks();
                });
            });

            using var server = new TestServer(webHostBuilder);

            // Act
            using var actualResponse = await server.CreateRequest("/monitor/live").GetAsync();

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }

        [Fact]
        public async Task CallingReady_Should_ReturnOKAndHealthy()
        {
            // Arrange
            var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();

                services.AddHealthChecks()
                    .AddLiveCheck();
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapLiveHealthChecks();
                    endpoints.MapReadyHealthChecks();
                });
            });

            using var server = new TestServer(webHostBuilder);

            // Act
            using var actualResponse = await server.CreateRequest("/monitor/ready").GetAsync();

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }
    }
}
