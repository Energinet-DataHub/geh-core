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

using System.Net;
using ExampleHost.FunctionApp01.FeatureManagement;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.FeatureManagement;

namespace ExampleHost.FunctionApp01.Functions;

public class FeatureManagementFunction
{
    private readonly IFeatureManager _featureManager;

    public FeatureManagementFunction(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    /// <summary>
    /// Demonstrate how we can use FeatureManager/FeatureManagerExtensions to switch on a feature flag.
    ///
    /// See the integration tests for this method for more on how it works, and how it can be tested.
    /// </summary>
    [Function(nameof(GetMessage))]
    public async Task<HttpResponseData> GetMessage(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "featuremanagement")]
        HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        var isFeatureEnabled = await _featureManager.UseGetMessagesAsync().ConfigureAwait(false);
        if (isFeatureEnabled)
        {
            // Perform logic when feature is enabled
            await response.WriteStringAsync("Enabled").ConfigureAwait(false);
            return response;
        }

        // Perform logic when feature is disabled
        await response.WriteStringAsync("Disabled").ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Demonstrate how we can use an app setting (see 'ExampleHostFixture') to disable an Azure Function.
    /// See also: https://docs.microsoft.com/en-us/azure/azure-functions/disable-function
    ///
    /// See the integration tests for this method for more on how it works, and how it can be tested.
    /// </summary>
    [Function(nameof(CreateMessage))]
    public HttpResponseData CreateMessage(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "featuremanagement")]
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
    /// <remarks>
    /// Similar functionality exists for Web App in the 'FeatureManagementController' class
    /// located in the 'ExampleHost.WebApi01' project.
    /// </remarks>
    [Function(nameof(GetFeatureFlagState))]
    public async Task<HttpResponseData> GetFeatureFlagState(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "featureflagstate/{featureFlagName}")]
        HttpRequestData request,
        string featureFlagName)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        var isFeatureEnabled = await _featureManager.IsEnabledAsync(featureFlagName).ConfigureAwait(false);
        if (isFeatureEnabled)
        {
            await response.WriteStringAsync("Enabled").ConfigureAwait(false);
            return response;
        }

        await response.WriteStringAsync("Disabled").ConfigureAwait(false);
        return response;
    }
}
