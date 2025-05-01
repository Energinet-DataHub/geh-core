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

using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;

/// <summary>
/// Extension methods for performing configuration in a <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// For use in a Function App isolated worker.
    /// Configures use of Azure App Configuration for feature flags only.
    /// </summary>
    public static IConfigurationBuilder AddAzureAppConfigurationForIsolatedWorker(this IConfigurationBuilder configBuilder)
    {
        var settings = configBuilder.Build();
        var appConfigEndpoint = settings["AppConfigEndpoint"]!
            ?? throw new InvalidConfigurationException($"Missing 'AppConfigEndpoint'.");

        configBuilder.AddAzureAppConfiguration(options =>
        {
            options
                .Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                // Using dummy key "_" to avoid loading other configuration than feature flags
                .Select("_")
                // Load all feature flags with no label.
                // Use the default refresh interval of 30 seconds.
                .UseFeatureFlags();
        });

        return configBuilder;
    }
}
