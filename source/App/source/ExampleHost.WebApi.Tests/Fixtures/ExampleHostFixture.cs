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
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.FunctionApp.TestCommon.AppConfiguration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using ExampleHost.WebApi.Tests.Integration;
using ExampleHost.WebApi01.Extensions.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixtures;

public class ExampleHostFixture : IAsyncLifetime
{
    public ExampleHostFixture()
    {
        var web02BaseUrl = "http://localhost:5001";
        var web01BaseUrl = "http://localhost:5000";

        IntegrationTestConfiguration = new IntegrationTestConfiguration();

        AppConfigurationManager = new AppConfigurationManager(
            IntegrationTestConfiguration.AppConfigurationEndpoint,
            IntegrationTestConfiguration.Credential);

        // We cannot use TestServer as this would not work with Application Insights.
        Web02Host = WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // Application Insights Telemetry
                    ["ApplicationInsights:ConnectionString"] = IntegrationTestConfiguration.ApplicationInsightsConnectionString,
                    // Logging to Application Insights
                    ["Logging:ApplicationInsights:LogLevel:Default"] = "Information",
                    // Subsystem-to-subsystem communication (client side)
                    [$"{SubsystemAuthenticationOptions.SectionName}:{nameof(SubsystemAuthenticationOptions.ApplicationIdUri)}"] = SubsystemAuthenticationOptionsForTests.ApplicationIdUri,
                    [$"{SubsystemAuthenticationOptions.SectionName}:{nameof(SubsystemAuthenticationOptions.Issuer)}"] = SubsystemAuthenticationOptionsForTests.Issuer,
                });
            })
            .UseStartup<WebApi02.Startup>()
            .UseUrls(web02BaseUrl)
            .Build();

        Web01Host = WebHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // Application Insights
                    ["ApplicationInsights:ConnectionString"] = IntegrationTestConfiguration.ApplicationInsightsConnectionString,
                    // Logging to Application Insights
                    ["Logging:ApplicationInsights:LogLevel:Default"] = "Information",
                    // Configure Azure App Configuration
                    [$"{AzureAppConfigurationOptions.SectionName}:{nameof(AzureAppConfigurationOptions.Endpoint)}"] = AppConfigurationManager.AppConfigEndpoint,
                    [$"{AzureAppConfigurationOptions.SectionName}:{nameof(AzureAppConfigurationOptions.FeatureFlagsRefreshIntervalInSeconds)}"] = "5",
                    // Configure local feature flag for test
                    [$"FeatureManagement:{FeatureManagementTests.LocalFeatureFlag}"] = "true",
                    // Subsystem-to-subsystem communication (client side)
                    [$"{WebApi02HttpClientsOptions.SectionName}:{nameof(WebApi02HttpClientsOptions.ApplicationIdUri)}"] = SubsystemAuthenticationOptionsForTests.ApplicationIdUri,
                    [$"{WebApi02HttpClientsOptions.SectionName}:{nameof(WebApi02HttpClientsOptions.ApiBaseAddress)}"] = web02BaseUrl,
                });

                // The 'Startup' class supported by ASp.NET Core doesn't have an method where we can
                // perform this configuration, so we have to perform it here (just as we have also added it to Programs.cs).
                var configuration = configBuilder.Build();
                configBuilder.AddAzureAppConfigurationForWebApp(configuration);
            })
            .UseStartup<WebApi01.Startup>()
            .UseUrls(web01BaseUrl)
            .Build();

        Web01HttpClient = new HttpClient
        {
            BaseAddress = new Uri(web01BaseUrl),
        };

        Web02HttpClient = new HttpClient
        {
            BaseAddress = new Uri(web02BaseUrl),
        };

        LogsQueryClient = new LogsQueryClient(IntegrationTestConfiguration.Credential);
    }

    public AppConfigurationManager AppConfigurationManager { get; }

    public HttpClient Web01HttpClient { get; }

    public HttpClient Web02HttpClient { get; }

    public LogsQueryClient LogsQueryClient { get; }

    public string LogAnalyticsWorkspaceId
        => IntegrationTestConfiguration.LogAnalyticsWorkspaceId;

    private IWebHost Web01Host { get; }

    private IWebHost Web02Host { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    public async Task InitializeAsync()
    {
        await Web02Host.StartAsync();
        await Web01Host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        Web01HttpClient.Dispose();
        await Web01Host.StopAsync();
        await Web02Host.StopAsync();
    }
}
