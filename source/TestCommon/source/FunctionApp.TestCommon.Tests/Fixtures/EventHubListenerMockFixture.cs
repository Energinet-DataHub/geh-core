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

using Azure.Core;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// This fixtures ensures we reuse retrieved configuration and
/// relevant instances, so we only have to retrieve an access token
/// and values in Key Vault one time.
/// It also ensures we use the local storage emulator for our blob container.
/// </summary>
public class EventHubListenerMockFixture : IAsyncLifetime
{
    public EventHubListenerMockFixture()
    {
        TestLogger = new TestDiagnosticsLogger();
        AzuriteManager = new AzuriteManager(useOAuth: true);
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public Uri BlobStorageServiceUri =>
        AzuriteManager.BlobStorageServiceUri;

    public string NamespaceName =>
        SingletonIntegrationTestConfiguration.Instance.EventHubNamespaceName;

    public string FullyQualifiedNamespace =>
        SingletonIntegrationTestConfiguration.Instance.EventHubFullyQualifiedNamespace;

    public AzureResourceManagementSettings ResourceManagementSettings =>
        SingletonIntegrationTestConfiguration.Instance.ResourceManagementSettings;

    public TokenCredential Credential =>
        SingletonIntegrationTestConfiguration.Instance.Credential;

    private AzuriteManager AzuriteManager { get; }

    public Task InitializeAsync()
    {
        AzuriteManager.StartAzurite();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        AzuriteManager.Dispose();

        return Task.CompletedTask;
    }
}
