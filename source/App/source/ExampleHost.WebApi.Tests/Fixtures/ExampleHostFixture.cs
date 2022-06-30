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

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures
{
    public class ExampleHostFixture : IAsyncLifetime
    {
        public ExampleHostFixture()
        {
            var web01BaseUrl = "http://localhost:5000";

            // We cannot use TestServer as this would not work with Application Insights.
            Web01Host = WebHost.CreateDefaultBuilder()
                      .UseStartup<WebApi01.Startup>()
                      .UseUrls(web01BaseUrl)
                      .Build();

            Web01HttpClient = new HttpClient
            {
                BaseAddress = new Uri(web01BaseUrl),
            };
        }

        public HttpClient Web01HttpClient { get; }

        private IWebHost Web01Host { get; }

        public async Task InitializeAsync()
        {
            await Web01Host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            Web01HttpClient.Dispose();
            await Web01Host.StopAsync();
        }
    }
}
