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

using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;
using ExampleHost.WebApi03;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures;

public class WebApi03HostFixture : IAsyncLifetime
{
    public WebApi03HostFixture()
    {
        IntegrationTestConfiguration = new IntegrationTestConfiguration();
        OpenIdJwtManager = new OpenIdJwtManager(IntegrationTestConfiguration.B2CSettings);

        var web03BaseUrl = "http://localhost:5002";
        Web03Host = WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // Authentication
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}"] = OpenIdJwtManager.ExternalMetadataAddress,
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}"] = OpenIdJwtManager.ExternalMetadataAddress,
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.BackendBffAppId)}"] = OpenIdJwtManager.TestBffAppId,
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.InternalMetadataAddress)}"] = OpenIdJwtManager.InternalMetadataAddress,
                });
            })
            .UseStartup<Startup>()
            .UseUrls(web03BaseUrl)
            .Build();

        Web03HttpClient = new HttpClient
        {
            BaseAddress = new Uri(web03BaseUrl),
        };
    }

    public HttpClient Web03HttpClient { get; }

    public OpenIdJwtManager OpenIdJwtManager { get; }

    private IWebHost Web03Host { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    public async Task InitializeAsync()
    {
        OpenIdJwtManager.StartServer();
        await Web03Host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        Web03HttpClient.Dispose();
        await Web03Host.StopAsync();

        OpenIdJwtManager.Dispose();
    }
}
