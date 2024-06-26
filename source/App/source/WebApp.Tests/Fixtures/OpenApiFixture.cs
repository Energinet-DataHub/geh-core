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

using System.Reflection;
using Asp.Versioning;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Energinet.DataHub.Core.App.WebApp.Tests.Fixtures;

public sealed class OpenApiFixture : IDisposable
{
    private readonly TestServer _server;

    public OpenApiFixture()
    {
        var webHostBuilder = CreateWebHostBuilder(3);
        _server = new TestServer(webHostBuilder);
        HttpClient = _server.CreateClient();
    }

    public HttpClient HttpClient { get; }

    public HttpClient GetClientWithApiVersionAndTitle(int apiVersion, string title = "dummy title")
    {
        var webHostBuilder = CreateWebHostBuilder(apiVersion, title);
        var server = new TestServer(webHostBuilder);
        return server.CreateClient();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private static IWebHostBuilder CreateWebHostBuilder(int apiVersion = 1, string title = "dummy title")
    {
        return new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSwaggerForWebApp(Assembly.GetExecutingAssembly(), swaggerUITitle: title)
                    .AddApiVersioningForWebApp(new ApiVersion(apiVersion, 0));
            })
            .Configure(app =>
            {
                app
                    .UseSwaggerForWebApp();
            });
    }
}
