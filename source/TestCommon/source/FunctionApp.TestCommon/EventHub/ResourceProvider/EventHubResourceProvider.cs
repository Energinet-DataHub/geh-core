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

using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.Resources;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Nito.AsyncEx;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;

/// <summary>
/// The resource provider and related builders encapsulates the creation of event hubs in an
/// existing Azure Event Hub namespace, and support creating the related client types as well.
///
/// The event hub names are build using a combination of the given name as well as a
/// random suffix per provider instance. This ensures we can easily identity resources from a certain
/// test run; and avoid name clashing if the test suite is executed by two identities at the same time.
///
/// Disposing the event hub resource provider will delete all created resources and dispose any created clients.
/// </summary>
public class EventHubResourceProvider : IAsyncDisposable
{
    public EventHubResourceProvider(ITestDiagnosticsLogger testLogger, string namespaceName, AzureResourceManagementSettings resourceManagementSettings)
    {
        TestLogger = testLogger
            ?? throw new ArgumentNullException(nameof(testLogger));
        NamespaceName = string.IsNullOrWhiteSpace(namespaceName)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(namespaceName))
            : namespaceName;
        ResourceManagementSettings = resourceManagementSettings
            ?? throw new ArgumentNullException(nameof(resourceManagementSettings));

        FullyQualifiedNamespace = $"{NamespaceName}.servicebus.windows.net";

        Credential = new DefaultAzureCredential();
        LazyEventHubNamespaceResource = new AsyncLazy<EventHubsNamespaceResource>(CreateEventHubNamespaceResourceAsync);

        RandomSuffix = $"{DateTimeOffset.UtcNow:yyyy.MM.ddTHH.mm.ss}-{Guid.NewGuid()}";
        EventHubResources = new Dictionary<string, EventHubResource>();
    }

    /// <summary>
    /// The name of the Event Hub Namespace under which Event Hubs are created.
    /// It is used to retrieve and create resources at the Azure Control Plane level.
    /// </summary>
    public string NamespaceName { get; }

    public AzureResourceManagementSettings ResourceManagementSettings { get; }

    /// <summary>
    /// The fully qualified namespace of the Event Hub Namespace under which Event Hubs are created.
    /// It is used when creating clients which communicates with the Event Hubs.
    /// </summary>
    public string FullyQualifiedNamespace { get; }

    /// <summary>
    /// Is used as part of the resource names.
    /// Allows us to identify resources created using the same instance (e.g. for debugging).
    /// </summary>
    public string RandomSuffix { get; }

    internal ITestDiagnosticsLogger TestLogger { get; }

    internal TokenCredential Credential { get; }

    internal AsyncLazy<EventHubsNamespaceResource> LazyEventHubNamespaceResource { get; }

    internal IDictionary<string, EventHubResource> EventHubResources { get; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore()
            .ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Build a event hub with a name based on <paramref name="eventHubNamePrefix"/> and <see cref="RandomSuffix"/>.
    /// </summary>
    /// <param name="eventHubNamePrefix">The event hub name will start with this name.</param>
    /// <returns>Event hub resource builder.</returns>
    public EventHubResourceBuilder BuildEventHub(string eventHubNamePrefix)
    {
        var eventHubName = BuildResourceName(eventHubNamePrefix);
        var createEventHubOptions = new EventHubData()
        {
            PartitionCount = 1,
        };

        return new EventHubResourceBuilder(this, eventHubName, createEventHubOptions);
    }

    private async Task<EventHubsNamespaceResource> CreateEventHubNamespaceResourceAsync()
    {
        var armClient = new ArmClient(Credential, ResourceManagementSettings.SubscriptionId);
        var resourceGroup = armClient.GetResourceGroupResource(
            ResourceGroupResource.CreateResourceIdentifier(
                ResourceManagementSettings.SubscriptionId,
                ResourceManagementSettings.ResourceGroup));

        return await resourceGroup.GetEventHubsNamespaceAsync(NamespaceName).ConfigureAwait(false);
    }

    private string BuildResourceName(string namePrefix)
    {
        return string.IsNullOrWhiteSpace(namePrefix)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(namePrefix))
            : $"{namePrefix}-{RandomSuffix}";
    }

    private async ValueTask DisposeAsyncCore()
    {
        foreach (var eventHubResource in EventHubResources)
        {
            // TODO: Dispose in parallel
            await eventHubResource.Value.DisposeAsync()
                .ConfigureAwait(false);
        }
    }
}
