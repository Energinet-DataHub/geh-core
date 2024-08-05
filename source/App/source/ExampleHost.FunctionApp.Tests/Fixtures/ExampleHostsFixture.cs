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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Monitor.Query;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Options;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using ExampleHost.FunctionApp01.Functions;
using Microsoft.Identity.Client;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Fixtures;

/// <summary>
/// Support testing flows between multiple Function App hosts.
/// </summary>
public class ExampleHostsFixture : IAsyncLifetime
{
    public ExampleHostsFixture()
    {
        TestLogger = new TestDiagnosticsLogger();

        AzuriteManager = new AzuriteManager();
        IntegrationTestConfiguration = new IntegrationTestConfiguration();
        ServiceBusResourceProvider = new ServiceBusResourceProvider(IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);

        HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
        LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());

        BffAppId = IntegrationTestConfiguration.Configuration.GetValue("AZURE-B2C-TESTBFF-APP-ID");
    }

    /// <summary>
    /// This is not the actual BFF but a test app registration that allows
    /// us to verify some of the JWT code.
    /// </summary>
    public string BffAppId { get; set; }

    public ITestDiagnosticsLogger TestLogger { get; }

    public LogsQueryClient LogsQueryClient { get; }

    public string LogAnalyticsWorkspaceId
        => IntegrationTestConfiguration.LogAnalyticsWorkspaceId;

    [NotNull]
    public FunctionAppHostManager? App01HostManager { get; private set; }

    [NotNull]
    public FunctionAppHostManager? App02HostManager { get; private set; }

    private AzuriteManager AzuriteManager { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

    private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

    public async Task InitializeAsync()
    {
        // => Storage emulator
        AzuriteManager.StartAzurite();

        // => Prepare host settings
        var localSettingsSnapshot = HostConfigurationBuilder.BuildLocalSettingsConfiguration();

        var port = 8000;
        var app01HostSettings = CreateAppHostSettings("ExampleHost.FunctionApp01", ref port);
        var app02HostSettings = CreateAppHostSettings("ExampleHost.FunctionApp02", ref port);

        // => App01 settings for authentication
        var app01BaseUrl = "http://localhost:8001";
        var externalMetadataAddress = $"https://login.microsoftonline.com/{IntegrationTestConfiguration.B2CSettings.Tenant}/v2.0/.well-known/openid-configuration";
        var internalMetadataAddress = $"{app01BaseUrl}/api/v2.0/.well-known/openid-configuration";

        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}", externalMetadataAddress);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}", externalMetadataAddress);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.BackendBffAppId)}", BffAppId);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.InternalMetadataAddress)}", internalMetadataAddress);

        // => Integration events
        app01HostSettings.ProcessEnvironmentVariables.Add("INTEGRATIONEVENT_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);
        app02HostSettings.ProcessEnvironmentVariables.Add("INTEGRATIONEVENT_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

        await ServiceBusResourceProvider
            .BuildTopic("integrationevent-topic")
                .Do(topicProperties =>
                {
                    app01HostSettings.ProcessEnvironmentVariables.Add("INTEGRATIONEVENT_TOPIC_NAME", topicProperties.Name);
                    app02HostSettings.ProcessEnvironmentVariables.Add("INTEGRATIONEVENT_TOPIC_NAME", topicProperties.Name);
                })
            .AddSubscription("integrationevent-app02-subscription")
                .Do(subscriptionProperties =>
                    app02HostSettings.ProcessEnvironmentVariables.Add("INTEGRATIONEVENT_SUBSCRIPTION_NAME", subscriptionProperties.SubscriptionName))
            .CreateAsync();

        // => Create and start host's
        App01HostManager = new FunctionAppHostManager(app01HostSettings, TestLogger);
        App02HostManager = new FunctionAppHostManager(app02HostSettings, TestLogger);

        StartHost(App01HostManager);
        StartHost(App02HostManager);
    }

    public async Task DisposeAsync()
    {
        App01HostManager.Dispose();
        App02HostManager.Dispose();

        AzuriteManager.Dispose();

        // => Service Bus
        await ServiceBusResourceProvider.DisposeAsync();
    }

    /// <summary>
    /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
    /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
    /// It is important that it is only attached while a test i active. Hence, it should be attached in
    /// the test class constructor; and detached in the test class Dispose method (using 'null').
    /// </summary>
    /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>; otherwise it should be 'null'.</param>
    public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        TestLogger.TestOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Get an access token that allows the "client app" to call the "backend app".
    /// </summary>
    public Task<AuthenticationResult> GetTokenAsync()
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

    /// <summary>
    /// Calls the <see cref="MockedTokenFunction"/> on "App01" to create an "internal token"
    /// and returns a 'Bearer' authentication header.
    /// </summary>
    public async Task<string> CreateAuthenticationHeaderWithNestedTokenAsync(params string[] roles)
    {
        var externalAuthenticationResult = await GetTokenAsync();

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                ExternalToken = externalAuthenticationResult.AccessToken,
                Roles = string.Join(',', roles),
            }),
            Encoding.UTF8,
            "application/json");

        using var tokenResponse = await App01HostManager.HttpClient.PostAsync(
            "api/token",
            jsonContent);

        var nestedToken = await tokenResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(nestedToken))
            throw new InvalidOperationException("Nested token was not created.");

        var authenticationHeader = $"Bearer {nestedToken}";
        return authenticationHeader;
    }

    private FunctionAppHostSettings CreateAppHostSettings(string csprojName, ref int port)
    {
        var buildConfiguration = GetBuildConfiguration();

        var appHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
        appHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\{csprojName}\\bin\\{buildConfiguration}\\net8.0";
        appHostSettings.Port = ++port;

        appHostSettings.ProcessEnvironmentVariables = new Dictionary<string, string>()
        {
            { "FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated" },
            { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
            // Application Insights Telemetry
            { "APPLICATIONINSIGHTS_CONNECTION_STRING", IntegrationTestConfiguration.ApplicationInsightsConnectionString },
            // Logging to Application Insights
            { "Logging__ApplicationInsights__LogLevel__Default", "Information" },
        };

        return appHostSettings;
    }

    private static void StartHost(FunctionAppHostManager hostManager)
    {
        IEnumerable<string> hostStartupLog;

        try
        {
            hostManager.StartHost();
        }
        catch (Exception)
        {
            // Function App Host failed during startup.
            // Exception has already been logged by host manager.
            hostStartupLog = hostManager.GetHostLogSnapshot();

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            // Rethrow
            throw;
        }

        // Function App Host started.
        hostStartupLog = hostManager.GetHostLogSnapshot();
    }

    private static string GetBuildConfiguration()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }
}
