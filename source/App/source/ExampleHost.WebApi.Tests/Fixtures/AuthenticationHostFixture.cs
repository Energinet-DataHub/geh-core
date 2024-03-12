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
using Microsoft.Identity.Client;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures
{
    public class AuthenticationHostFixture : IAsyncLifetime
    {
        public AuthenticationHostFixture()
            : this("http://localhost:5003", false) { }

        protected AuthenticationHostFixture(string web04BaseUrl, bool supportNestedTokens)
        {
            IntegrationTestConfiguration = new IntegrationTestConfiguration();

            Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", IntegrationTestConfiguration.ApplicationInsightsConnectionString);

            var innerMetadataArg = $"--innerMetadata={Metadata}";
            var outerMetadataArg = $"--outerMetadata=";
            var audienceArg = $"--audience={Audience}";

            if (supportNestedTokens)
            {
                outerMetadataArg = $"--outerMetadata={web04BaseUrl}/webapi04/v2.0/.well-known/openid-configuration";
            }

            // We cannot use TestServer as this would not work with Application Insights.
            Web04Host = WebHost.CreateDefaultBuilder(new[]
                {
                    innerMetadataArg,
                    outerMetadataArg,
                    audienceArg,
                })
                .UseStartup<WebApi04.Startup>()
                .UseUrls(web04BaseUrl)
                .Build();

            Web04HttpClient = new HttpClient
            {
                BaseAddress = new Uri(web04BaseUrl),
            };
        }

        public string Metadata => $"https://login.microsoftonline.com/{IntegrationTestConfiguration.B2CSettings.Tenant}/v2.0/.well-known/openid-configuration";

        public string Audience => IntegrationTestConfiguration.B2CSettings.BackendAppId;

        public HttpClient Web04HttpClient { get; }

        /// <summary>
        /// Get an access token that allows the "client app" to call the "backend app".
        /// </summary>
        public Task<AuthenticationResult> GetTokenAsync()
        {
            // TODO: Might have to create another client app for these tests(?)
            var clientId = IntegrationTestConfiguration.B2CSettings.ServicePrincipalId;
            var clientSecret = IntegrationTestConfiguration.B2CSettings.ServicePrincipalSecret;

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(authorityUri: $"https://login.microsoftonline.com/{IntegrationTestConfiguration.B2CSettings.Tenant}")
                .Build();

            // TODO: Might have to create another backend app for these tests(?)
            return confidentialClientApp
                .AcquireTokenForClient(scopes: new[] { $"{IntegrationTestConfiguration.B2CSettings.BackendAppId}/.default" })
                .ExecuteAsync();
        }

        private IWebHost Web04Host { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        public async Task InitializeAsync()
        {
            await Web04Host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            Web04HttpClient.Dispose();
            await Web04Host.StopAsync();
        }
    }
}
