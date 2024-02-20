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
using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NodaTime;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Fixtures
{
    public sealed class HealthChecksFixture : IDisposable
    {
        private readonly TestServer _server;

        public HealthChecksFixture()
        {
            var webHostBuilder = CreateWebHostBuilder();
            _server = new TestServer(webHostBuilder);
            HttpClient = _server.CreateClient();
        }

        public HttpClient HttpClient { get; }

        public void Dispose()
        {
            _server.Dispose();
        }

        private static IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddOptions<DatabricksSqlStatementOptions>().Configure(options =>
                    {
                        options.WarehouseId = "baz";
                        options.WorkspaceToken = "bar";
                        options.WorkspaceUrl = "https://foo.com";
                        options.DatabricksHealthCheckStartHour = 0;
                        options.DatabricksHealthCheckEndHour = 23;
                    });

                    services.AddRouting();
                    services.AddScoped(typeof(IClock), _ => SystemClock.Instance);

                    RegisterHttpClientFactoryMock(services);

                    services.AddHealthChecks()
                        .AddLiveCheck()
                        .AddDatabricksSqlStatementApiHealthCheck();
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
        }

        private static void RegisterHttpClientFactoryMock(IServiceCollection services)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            services.AddScoped<HttpClient>(_ => httpClient);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(Options.DefaultName))
                .Returns(() => httpClient);

            services.AddScoped<IHttpClientFactory>(_ => httpClientFactoryMock.Object);
        }
    }
}
