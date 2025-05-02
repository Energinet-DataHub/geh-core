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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Monitor.Query;
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Energinet.DataHub.Core.FunctionApp.TestCommon.AppConfiguration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using ExampleHost.FunctionApp01.Common;
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
        ServiceBusResourceProvider = new ServiceBusResourceProvider(TestLogger, IntegrationTestConfiguration.ServiceBusFullyQualifiedNamespace, IntegrationTestConfiguration.Credential);

        HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
        LogsQueryClient = new LogsQueryClient(new DefaultAzureCredential());

        OpenIdJwtManager = new OpenIdJwtManager(IntegrationTestConfiguration.B2CSettings, openIdServerPort: 1052);

        AppConfigurationManager = new AppConfigurationManager(
            IntegrationTestConfiguration.AppConfigurationEndpoint,
            IntegrationTestConfiguration.Credential);
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public LogsQueryClient LogsQueryClient { get; }

    public string LogAnalyticsWorkspaceId
        => IntegrationTestConfiguration.LogAnalyticsWorkspaceId;

    public OpenIdJwtManager OpenIdJwtManager { get; }

    public AppConfigurationManager AppConfigurationManager { get; }

    [NotNull]
    public FunctionAppHostManager? App01HostManager { get; private set; }

    [NotNull]
    public FunctionAppHostManager? App02HostManager { get; private set; }

    /// <summary>
    /// The setting name of the <see cref="FeatureFlags.Names.UseGetMessage"/> feature flag.
    /// </summary>
    public string UseGetMessageSettingName => $"{FeatureFlags.ConfigurationPrefix}{FeatureFlags.Names.UseGetMessage}";

    /// <summary>
    /// The setting name of the CreateMessage (function) disabled flag.
    /// </summary>
    public string CreateMessageDisabledSettingName => "AzureWebJobs.CreateMessage.Disabled";

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

        // => App01 settings for Azure App Configuration (used for feature flags)
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{AzureAppConfigurationOptions.SectionName}:{nameof(AzureAppConfigurationOptions.Endpoint)}", AppConfigurationManager.AppConfigEndpoint);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{AzureAppConfigurationOptions.SectionName}:{nameof(AzureAppConfigurationOptions.FeatureFlagsRefreshIntervalInSeconds)}", "5");
        // => App01 settings for Feature flags
        app01HostSettings.ProcessEnvironmentVariables.Add(UseGetMessageSettingName, "false");
        // => App01 settings for Function Disabled flags
        app01HostSettings.ProcessEnvironmentVariables.Add(CreateMessageDisabledSettingName, "false");

        // => App01 settings for authentication
        OpenIdJwtManager.StartServer();
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.MitIdExternalMetadataAddress)}", OpenIdJwtManager.ExternalMetadataAddress);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.ExternalMetadataAddress)}", OpenIdJwtManager.ExternalMetadataAddress);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.BackendBffAppId)}", OpenIdJwtManager.TestBffAppId);
        app01HostSettings.ProcessEnvironmentVariables.Add(
            $"{UserAuthenticationOptions.SectionName}:{nameof(UserAuthenticationOptions.InternalMetadataAddress)}", OpenIdJwtManager.InternalMetadataAddress);

        // => Integration events
        app01HostSettings.ProcessEnvironmentVariables.Add("INTEGRATIONEVENT_FULLY_QUALIFIED_NAMESPACE", ServiceBusResourceProvider.FullyQualifiedNamespace);

        // Using Identity-based connections to establish a connection to the Service Bus for the ServiceBusTrigger
        // https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp#identity-based-connections
        var integrationEventSettingPrefix = "INTEGRATIONEVENT";
        app02HostSettings.ProcessEnvironmentVariables.Add($"{integrationEventSettingPrefix}_SETTING_PREFIX", integrationEventSettingPrefix);
        app02HostSettings.ProcessEnvironmentVariables.Add($"{integrationEventSettingPrefix}:fullyQualifiedNamespace", ServiceBusResourceProvider.FullyQualifiedNamespace);

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

        OpenIdJwtManager.Dispose();
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

    private FunctionAppHostSettings CreateAppHostSettings(string csprojName, ref int port)
    {
        var buildConfiguration = GetBuildConfiguration();

        var appHostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();
        appHostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\{csprojName}\\bin\\{buildConfiguration}\\net9.0";
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
