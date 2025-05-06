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

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace ExampleHost.WebApi01.Controllers;

[ApiController]
[Route("webapi01/[controller]")]
public class FeatureManagementController : ControllerBase
{
    private readonly IFeatureManager _featureManager;

    public FeatureManagementController(IFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }

    /// <summary>
    /// Used by tests to prove how Feature Manager can be used with Azure App Configuration
    /// to refresh feature flags at runtime.
    ///
    /// See the integration tests for this method for more on how it works.
    /// </summary>
    /// <remarks>
    /// Similar functionality exists for Function App in the 'FeatureManagementFunction' class
    /// located in the 'ExampleHost.FunctionApp01' project.
    /// </remarks>
    [HttpGet("{featureFlagName}")]
    public async Task<string> GetFeatureFlagState(string featureFlagName)
    {
        var isFeatureEnabled = await _featureManager.IsEnabledAsync(featureFlagName).ConfigureAwait(false);
        return isFeatureEnabled
            ? "Enabled"
            : "Disabled";
    }
}
