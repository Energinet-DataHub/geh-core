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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures
{
    public sealed class AuthorizationHostFixture : IAsyncLifetime
    {
        public AuthorizationHostFixture()
        {
            var web03BaseUrl = "http://localhost:5002";

            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", IntegrationTestConfiguration.ApplicationInsightsConnectionString);

            // We cannot use TestServer as this would not work with Application Insights.
            Web03Host = WebHost.CreateDefaultBuilder()
                .UseStartup<WebApi03.Startup>()
                .UseUrls(web03BaseUrl)
                .Build();

            Web03HttpClient = new HttpClient
            {
                BaseAddress = new Uri(web03BaseUrl),
            };
        }

        public HttpClient Web03HttpClient { get; }

        private IWebHost Web03Host { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        public async Task InitializeAsync()
        {
            await Web03Host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            Web03HttpClient.Dispose();
            await Web03Host.StopAsync();
        }
    }
}
