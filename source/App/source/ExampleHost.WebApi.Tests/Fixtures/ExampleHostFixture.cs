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

using Azure.Identity;
using Azure.Monitor.Query;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures
{
    public class ExampleHostFixture : IAsyncLifetime
    {
        public ExampleHostFixture()
        {
            var web03BaseUrl = "http://localhost:5002";
            var web02BaseUrl = "http://localhost:5001";
            var web01BaseUrl = "http://localhost:5000";

            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);

            // We cannot use TestServer as this would not work with Application Insights.
            Web03Host = WebHost.CreateDefaultBuilder()
                .UseStartup<WebApi03.Startup>()
                .UseUrls(web03BaseUrl)
                .Build();

            Web02Host = WebHost.CreateDefaultBuilder()
                .UseStartup<WebApi02.Startup>()
                .UseUrls(web02BaseUrl)
                .Build();

            Environment.SetEnvironmentVariable(WebApi01.Common.EnvironmentSettingNames.WebApi02BaseUrl, web02BaseUrl);
            Web01Host = WebHost.CreateDefaultBuilder()
                .UseStartup<WebApi01.Startup>()
                .UseUrls(web01BaseUrl)
                .Build();

            Web01HttpClient = new HttpClient
            {
                BaseAddress = new Uri(web01BaseUrl),
            };

            Web03HttpClient = new HttpClient
            {
                BaseAddress = new Uri(web03BaseUrl),
            };

            LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
        }

        public HttpClient Web01HttpClient { get; }

        public HttpClient Web03HttpClient { get; }

        public LogsQueryClient LogsQueryClient { get; }

        public string LogAnalyticsWorkspaceId
            => IntegrationTestConfiguration.LogAnalyticsWorkspaceId;

        private IWebHost Web01Host { get; }

        private IWebHost Web02Host { get; }

        private IWebHost Web03Host { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        public async Task InitializeAsync()
        {
            await Web03Host.StartAsync();
            await Web02Host.StartAsync();
            await Web01Host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            Web01HttpClient.Dispose();
            await Web01Host.StopAsync();
            await Web02Host.StopAsync();
            await Web03Host.StopAsync();
        }
    }
}
