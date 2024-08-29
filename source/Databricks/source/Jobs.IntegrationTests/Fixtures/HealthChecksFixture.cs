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

using Energinet.DataHub.Core.App.Common.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Energinet.DataHub.Core.Databricks.Jobs.IntegrationTests.Fixtures;

public sealed class HealthChecksFixture : IDisposable
{
    private readonly TestServer _server;

    public HealthChecksFixture()
    {
        var integrationTestConfiguration = new IntegrationTestConfiguration();
        var webHostBuilder = CreateWebHostBuilder(integrationTestConfiguration);
        _server = new TestServer(webHostBuilder);

        HttpClient = _server.CreateClient();
    }

    public HttpClient HttpClient { get; }

    public void Dispose()
    {
        _server.Dispose();
    }

    private static IWebHostBuilder CreateWebHostBuilder(IntegrationTestConfiguration integrationTestConfiguration)
    {
        return new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        [$"{nameof(DatabricksJobsOptions.WorkspaceUrl)}"]
                            = integrationTestConfiguration.DatabricksSettings.WorkspaceUrl,
                        [$"{nameof(DatabricksJobsOptions.WorkspaceToken)}"]
                            = integrationTestConfiguration.DatabricksSettings.WorkspaceAccessToken,
                        [$"{nameof(DatabricksJobsOptions.WarehouseId)}"]
                            = integrationTestConfiguration.DatabricksSettings.WarehouseId,
                    })
                    .Build();

                services.AddRouting();
                services.AddScoped(typeof(IClock), _ => SystemClock.Instance);

                services.AddDatabricksJobs(configuration);

                services.AddHealthChecks()
                    .AddLiveCheck()
                    .AddDatabricksJobsApiHealthCheck();
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    // Databricks Jobs health check is registered for "ready" endpoint
                    endpoints.MapReadyHealthChecks();
                });
            });
    }
}
