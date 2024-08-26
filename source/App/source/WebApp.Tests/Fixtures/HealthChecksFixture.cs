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

using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.App.WebApp.Tests.Fixtures;

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
                services.AddRouting();

                services.AddHealthChecksForWebApp();
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapLiveHealthChecks();
                    endpoints.MapReadyHealthChecks();
                    endpoints.MapStatusHealthChecks();
                });
            });
    }
}
