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

using Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.AppSettings;
using Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.Fixtures
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
                    const string workspaceUrl = "https://foo.com";
                    const string workspaceToken = "bar";
                    const string warehouseId = "baz";
                    services.AddOptions<DatabricksJobsOptions>().Configure(options =>
                    {
                        options.WarehouseId = warehouseId;
                        options.WorkspaceToken = workspaceToken;
                        options.WorkspaceUrl = workspaceUrl;
                    });

                    services.AddRouting();
                    // services.AddHttpClient();
                    services.AddScoped<IJobsApiClient, JobsApiClient>();
                    services.AddScoped(typeof(IClock), _ => SystemClock.Instance);

                    services.AddHealthChecks().AddDatabricksJobsApiHealthCheck(
                            _ => new DatabricksJobsOptions
                        {
                            DatabricksHealthCheckStartHour = 6,
                            DatabricksHealthCheckEndHour = 20,
                            WorkspaceUrl = "https://fake",
                            WarehouseId = "id",
                            WorkspaceToken = "token",
                        });
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
    }
}
