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

using System.Text.RegularExpressions;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Microsoft.Azure.Management.EventHub;
using Microsoft.Azure.Management.EventHub.Models;
using Microsoft.Identity.Client;
using Microsoft.Rest;
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
    public EventHubResourceProvider(string connectionString, AzureResourceManagementSettings resourceManagementSettings, ITestDiagnosticsLogger testLogger)
    {
        ConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString))
            : connectionString;
        ResourceManagementSettings = resourceManagementSettings
            ?? throw new ArgumentNullException(nameof(resourceManagementSettings));
        TestLogger = testLogger
            ?? throw new ArgumentNullException(nameof(testLogger));

        EventHubNamespace = GetEventHubNamespace(ConnectionString);
        LazyManagementClient = new AsyncLazy<IEventHubManagementClient>(CreateManagementClientAsync);

        RandomSuffix = $"{DateTimeOffset.UtcNow:yyyy.MM.ddTHH.mm.ss}-{Guid.NewGuid()}";
        EventHubResources = new Dictionary<string, EventHubResource>();
    }

    public string ConnectionString { get; }

    public AzureResourceManagementSettings ResourceManagementSettings { get; }

    public string EventHubNamespace { get; }

    /// <summary>
    /// Is used as part of the resource names.
    /// Allows us to identify resources created using the same instance (e.g. for debugging).
    /// </summary>
    public string RandomSuffix { get; }

    internal ITestDiagnosticsLogger TestLogger { get; }

    internal IDictionary<string, EventHubResource> EventHubResources { get; }

    internal AsyncLazy<IEventHubManagementClient> LazyManagementClient { get; }

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
        var createEventHubOptions = new Eventhub()
        {
            MessageRetentionInDays = 1,
            PartitionCount = 1,
        };

        return new EventHubResourceBuilder(this, eventHubName, createEventHubOptions);
    }

    private static string GetEventHubNamespace(string eventHubConnectionString)
    {
        // The connection string is similar to a service bus connection string.
        // Example connection string: 'Endpoint=sb://xxx.servicebus.windows.net/;'
        var namespaceMatchPattern = @"Endpoint=sb://(.*?).servicebus.windows.net/";
        var match = Regex.Match(eventHubConnectionString, namespaceMatchPattern, RegexOptions.IgnoreCase);
        var eventHubNamespace = match.Groups[1].Value;

        return eventHubNamespace;
    }

    private async Task<IEventHubManagementClient> CreateManagementClientAsync()
    {
        var authenticationResult = await GetTokenAsync().ConfigureAwait(false);
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

    private string BuildResourceName(string namePrefix)
    {
        return string.IsNullOrWhiteSpace(namePrefix)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(namePrefix))
            : $"{namePrefix}-{RandomSuffix}";
    }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
    private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
    {
        foreach (var eventHubResource in EventHubResources)
        {
            // TODO: Dispose in parallel
            await eventHubResource.Value.DisposeAsync()
                .ConfigureAwait(false);
        }

        var managementClient = await LazyManagementClient
            .ConfigureAwait(false);
        managementClient.Dispose();
    }
}
