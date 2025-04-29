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

using System.Net;
using ExampleHost.FunctionApp01.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

namespace ExampleHost.FunctionApp01.Functions;

public class FeatureManagementFunction
{
    private readonly IConfigurationRefresherProvider _configurationRefresherProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IVariantFeatureManagerSnapshot _variantFeatureManagerSnapshot;

    public FeatureManagementFunction(
        IConfigurationRefresherProvider configurationRefresherProvider,
        IFeatureManager featureManager,
        IVariantFeatureManagerSnapshot variantFeatureManagerSnapshot)
    {
        _configurationRefresherProvider = configurationRefresherProvider;
        _featureManager = featureManager;
        _variantFeatureManagerSnapshot = variantFeatureManagerSnapshot;
    }

    /// <summary>
    /// Demonstrate how we can use FeatureManager to switch on a feature flag.
    ///
    /// See the integration tests for this method for more on how it works, and how it can be tested.
    /// </summary>
    [Function(nameof(GetMessage))]
    public async Task<HttpResponseData> GetMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "message")]
        HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        var isFeatureEnabled = await _featureManager.IsEnabledAsync(nameof(FeatureFlags.Names.UseGetMessage)).ConfigureAwait(false);
        if (isFeatureEnabled)
        {
            await response.WriteStringAsync("Enabled").ConfigureAwait(false);
            return response;
        }

        await response.WriteStringAsync("Disabled").ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Demonstrate how we can use an app setting (see local.settings.json) to disable an Azure Function.
    /// See also: https://docs.microsoft.com/en-us/azure/azure-functions/disable-function
    ///
    /// See the integration tests for this method for more on how it works, and how it can be tested.
    /// </summary>
    [Function(nameof(CreateMessage))]
    public HttpResponseData CreateMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "message")]
        HttpRequestData request)
    {
        return request.CreateResponse(HttpStatusCode.Accepted);
    }

    /// <summary>
    /// Used by tests to prove how Feature Manager can be used with Azure App Configuration
    /// to refresh feature flags at runtime.
    ///
    /// See the integration tests for this method for more on how it works.
    /// </summary>
    [Function(nameof(GetFeatureFlagState))]
    public async Task<HttpResponseData> GetFeatureFlagState(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "featureflagstate/{featureFlagName}")]
        HttpRequestData request,
        string featureFlagName)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        // TODO: Refactor
        if (featureFlagName == "Azure")
            featureFlagName = "edi-integrationtests/disabled-feature-flag";

        // TODO: Refactor
        await _configurationRefresherProvider.Refreshers.First().TryRefreshAsync().ConfigureAwait(false);
        var isFeatureEnabled = await _variantFeatureManagerSnapshot.IsEnabledAsync(featureFlagName).ConfigureAwait(false);
        if (isFeatureEnabled)
        {
            await response.WriteStringAsync("Enabled").ConfigureAwait(false);
            return response;
        }

        await response.WriteStringAsync("Disabled").ConfigureAwait(false);
        return response;
    }
}
