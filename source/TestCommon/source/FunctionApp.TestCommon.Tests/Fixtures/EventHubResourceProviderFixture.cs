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

using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.Resources;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// This fixtures ensures we reuse retrieved configuration and
/// relevant instances, so we only have to retrieve an access token
/// and values in Key Vault one time.
/// </summary>
public class EventHubResourceProviderFixture : IAsyncLifetime
{
    public EventHubResourceProviderFixture()
    {
        TestLogger = new TestDiagnosticsLogger();
        NamespaceName = SingletonIntegrationTestConfiguration.Instance.EventHubNamespaceName;
        FullyQualifiedNamespace = SingletonIntegrationTestConfiguration.Instance.EventHubFullyQualifiedNamespace;
        ResourceManagementSettings = SingletonIntegrationTestConfiguration.Instance.ResourceManagementSettings;
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public string NamespaceName { get; }

    public string FullyQualifiedNamespace { get; }

    public AzureResourceManagementSettings ResourceManagementSettings { get; }

    [NotNull]
    public EventHubsNamespaceResource? EventHubNamespaceResource { get; private set; }

    public async Task InitializeAsync()
    {
        EventHubNamespaceResource = await CreateEventHubNamespaceResourceAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task<EventHubsNamespaceResource> CreateEventHubNamespaceResourceAsync()
    {
        var credential = new DefaultAzureCredential();
        var armClient = new ArmClient(credential, ResourceManagementSettings.SubscriptionId);
        var resourceGroup = armClient.GetResourceGroupResource(
            ResourceGroupResource.CreateResourceIdentifier(
                ResourceManagementSettings.SubscriptionId,
                ResourceManagementSettings.ResourceGroup));

        return await resourceGroup.GetEventHubsNamespaceAsync(NamespaceName).ConfigureAwait(false);
    }
}
