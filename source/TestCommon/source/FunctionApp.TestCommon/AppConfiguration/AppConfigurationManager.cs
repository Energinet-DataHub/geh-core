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
using Azure;
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
    public async Task SetFeatureFlagAsync(string featureFlagName, bool isEnabled)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagName);

        await _client
            .SetConfigurationSettingAsync(
                new FeatureFlagConfigurationSetting(featureFlagName, isEnabled))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Delete feature flag if it exists.
    /// </summary>
    public async Task DeleteFeatureFlagAsync(string featureFlagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagName);

        await _client
            .DeleteConfigurationSettingAsync(
                key: $"{FeatureFlagConfigurationSetting.KeyPrefix}{featureFlagName}")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Get feature flag if it exists; otherwise throws exception.
    /// </summary>
    public async Task<bool> GetFeatureFlagStateAsync(string featureFlagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagName);

        try
        {
            var configSetting = await _client
                .GetConfigurationSettingAsync(
                    key: $"{FeatureFlagConfigurationSetting.KeyPrefix}{featureFlagName}")
                .ConfigureAwait(false);

            var featureFlagSetting = configSetting.Value as FeatureFlagConfigurationSetting;
            return featureFlagSetting!.IsEnabled;
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == (int)HttpStatusCode.NotFound)
                throw new ArgumentException(message: "Invalid feature flag name.", paramName: nameof(featureFlagName));

            throw;
        }
    }
}
