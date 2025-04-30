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
using Azure.Data.AppConfiguration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.AppConfiguration;

/// <summary>
/// A manager for managing feature flags in Azure App Configuration from integration tests.
/// </summary>
public class AppConfigurationManager
{
    private readonly ConfigurationClient _client;

    public AppConfigurationManager(string appConfigEndpoint, TokenCredential credential)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appConfigEndpoint);

        AppConfigEndpoint = appConfigEndpoint;
        _client = new(new Uri(appConfigEndpoint), credential);
    }

    public string AppConfigEndpoint { get; }

    /// <summary>
    /// Creates feature flag if it doesn't exist, or overwrites the existing value.
    /// </summary>
    public async Task SetFeatureFlagAsync(string featureFlagName, bool isEnabled, string? label = default)
    {
        await _client.SetConfigurationSettingAsync(new FeatureFlagConfigurationSetting(featureFlagName, isEnabled, label)).ConfigureAwait(false);
    }

    /// <summary>
    /// Delete feature flag if it exists.
    /// </summary>
    public async Task DeleteFeatureFlagAsync(string featureFlagName, bool isEnabled, string? label = default)
    {
        await _client.DeleteConfigurationSettingAsync(new FeatureFlagConfigurationSetting(featureFlagName, isEnabled, label)).ConfigureAwait(false);
    }
}
