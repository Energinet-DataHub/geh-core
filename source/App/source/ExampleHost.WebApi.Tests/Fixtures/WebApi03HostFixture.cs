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

using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using ExampleHost.WebApi03;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures;

public class WebApi03HostFixture : IAsyncLifetime
{
    public WebApi03HostFixture()
    {
        IntegrationTestConfiguration = new IntegrationTestConfiguration();
        BffAppId = IntegrationTestConfiguration.Configuration.GetValue("AZURE-B2C-TESTBFF-APP-ID");
        InternalTokenServer = new TokenMockServer();

        var web03BaseUrl = "http://localhost:5002";
        Web03Host = WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                var externalMetadataAddress = $"https://login.microsoftonline.com/{IntegrationTestConfiguration.B2CSettings.Tenant}/v2.0/.well-known/openid-configuration";
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // Authentication
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}"] = externalMetadataAddress,
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}"] = externalMetadataAddress,
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.BackendBffAppId)}"] = BffAppId,
                    [$"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.InternalMetadataAddress)}"] = InternalTokenServer.MetadataAddress,
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

    /// <summary>
    /// This is not the actual BFF but a test app registration that allows
    /// us to verify some of the JWT code.
    /// </summary>
    private string BffAppId { get; }

    private IWebHost Web03Host { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    private TokenMockServer InternalTokenServer { get; }

    public async Task InitializeAsync()
    {
        await Web03Host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        Web03HttpClient.Dispose();
        await Web03Host.StopAsync();

        InternalTokenServer.Dispose();
    }

    /// <summary>
    /// Calls the <see cref="InternalTokenServer"/> on to create an "internal token"
    /// and returns a 'Bearer' authentication header.
    /// </summary>
    public async Task<string> CreateAuthenticationHeaderWithNestedTokenAsync(params string[] roles)
    {
        var externalAuthenticationResult = await GetTokenAsync();

        var internalToken = InternalTokenServer.GetToken(externalAuthenticationResult.AccessToken, roles);
        if (string.IsNullOrWhiteSpace(internalToken))
            throw new InvalidOperationException("Nested token was not created.");

        var authenticationHeader = $"Bearer {internalToken}";
        return authenticationHeader;
    }

    /// <summary>
    /// Get an access token that allows the "client app" to call the "backend app".
    /// </summary>
    private Task<AuthenticationResult> GetTokenAsync()
    {
        var confidentialClientApp = ConfidentialClientApplicationBuilder
            .Create(IntegrationTestConfiguration.B2CSettings.ServicePrincipalId)
            .WithClientSecret(IntegrationTestConfiguration.B2CSettings.ServicePrincipalSecret)
            .WithAuthority(authorityUri: $"https://login.microsoftonline.com/{IntegrationTestConfiguration.B2CSettings.Tenant}")
            .Build();

        return confidentialClientApp
            .AcquireTokenForClient(scopes: new[] { $"{BffAppId}/.default" })
            .ExecuteAsync();
    }
}
