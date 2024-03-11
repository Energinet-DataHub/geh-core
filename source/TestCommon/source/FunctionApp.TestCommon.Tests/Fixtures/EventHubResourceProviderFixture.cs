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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Microsoft.Azure.Management.EventHub;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// This fixtures ensures we reuse <see cref="ConnectionString"/> and
/// relevant instances, so we only have to retrieve an access token
/// and values in Key Vault one time.
/// </summary>
public class EventHubResourceProviderFixture : IAsyncLifetime
{
    public EventHubResourceProviderFixture()
    {
        TestLogger = new TestDiagnosticsLogger();

        var integrationTestConfiguration = new IntegrationTestConfiguration();
        ConnectionString = integrationTestConfiguration.EventHubConnectionString;
        ResourceManagementSettings = integrationTestConfiguration.ResourceManagementSettings;
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public string ConnectionString { get; }

    public AzureResourceManagementSettings ResourceManagementSettings { get; }

    [NotNull]
    public IEventHubManagementClient? ManagementClient { get; private set; }

    public async Task InitializeAsync()
    {
        ManagementClient = await CreateManagementClientAsync();
    }

    public Task DisposeAsync()
    {
        ManagementClient.Dispose();

        return Task.CompletedTask;
    }

    private async Task<IEventHubManagementClient> CreateManagementClientAsync()
    {
        var authenticationResult = await GetTokenAsync();
        var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken);
        return new EventHubManagementClient(tokenCredentials)
        {
            SubscriptionId = ResourceManagementSettings.SubscriptionId,
        };
    }

    private Task<AuthenticationResult> GetTokenAsync()
    {
        var confidentialClientApp = ConfidentialClientApplicationBuilder
            .Create(ResourceManagementSettings.ClientId)
            .WithClientSecret(ResourceManagementSettings.ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{ResourceManagementSettings.TenantId}")
            .Build();

        return confidentialClientApp
            .AcquireTokenForClient(new[] { $"https://management.core.windows.net/.default" })
            .ExecuteAsync();
    }
}
