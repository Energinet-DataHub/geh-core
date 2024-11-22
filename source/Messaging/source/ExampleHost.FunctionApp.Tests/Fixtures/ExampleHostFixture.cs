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

using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace ExampleHost.FunctionApp.Tests.Fixtures;

public class ExampleHostFixture : FunctionAppFixture
{
    public ExampleHostFixture()
    {
        var credentials = new DefaultAzureCredential();
        AzuriteManager = new AzuriteManager(useOAuth: true);
        IntegrationTestConfiguration = new IntegrationTestConfiguration(credentials);
        ServiceBusResourceProvider = new ServiceBusResourceProvider(
            TestLogger,
            IntegrationTestConfiguration.ServiceBusFullyQualifiedNamespace,
            credentials);

        BlobContainerName = "examplehost";
        BlobServiceClient = new BlobServiceClient(AzuriteManager.BlobStorageServiceUri, credentials);
    }

    /// <summary>
    /// Topic resource for integration events.
    /// </summary>
    [NotNull]
    public TopicResource? TopicResource { get; private set; }

    public string BlobContainerName { get; }

    public BlobServiceClient BlobServiceClient { get; }

    private AzuriteManager AzuriteManager { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

    protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
    {
        const string csprojname = "ExampleHost.FunctionApp";
        var buildConfiguration = GetBuildConfiguration();

        hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\{csprojname}\\bin\\{buildConfiguration}\\net8.0";
    }

    protected override void OnConfigureEnvironment()
    {
        Environment.SetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated");
        // Storage emulator
        Environment.SetEnvironmentVariable("AzureWebJobsStorage", AzuriteManager.FullConnectionString);
        // Application Insights
        Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", IntegrationTestConfiguration.ApplicationInsightsConnectionString);
        // ServiceBus Namespace
        Environment.SetEnvironmentVariable($"{ServiceBusNamespaceOptions.SectionName}__{nameof(ServiceBusNamespaceOptions.FullyQualifiedNamespace)}", IntegrationTestConfiguration.ServiceBusFullyQualifiedNamespace);
        // Dead-letter logging
        Environment.SetEnvironmentVariable($"{BlobDeadLetterLoggerOptions.SectionName}__{nameof(BlobDeadLetterLoggerOptions.StorageAccountUrl)}", AzuriteManager.BlobStorageServiceUri.OriginalString);
        Environment.SetEnvironmentVariable($"{BlobDeadLetterLoggerOptions.SectionName}__{nameof(BlobDeadLetterLoggerOptions.ContainerName)}", BlobContainerName);
    }

    protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
    {
        // Storage emulator
        AzuriteManager.StartAzurite();

        // ServiceBus entities
        TopicResource = await ServiceBusResourceProvider
            .BuildTopic("integration-events")
            .SetEnvironmentVariableToTopicName($"{IntegrationEventsOptions.SectionName}__{nameof(IntegrationEventsOptions.TopicName)}")
            .AddSubscription("subscription")
            .SetEnvironmentVariableToSubscriptionName($"{IntegrationEventsOptions.SectionName}__{nameof(IntegrationEventsOptions.SubscriptionName)}")
            .CreateAsync();
    }

    protected override async Task OnDisposeFunctionAppDependenciesAsync()
    {
        AzuriteManager.Dispose();
        await ServiceBusResourceProvider.DisposeAsync();
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
