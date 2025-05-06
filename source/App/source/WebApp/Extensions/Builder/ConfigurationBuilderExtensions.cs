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
using Azure.Identity;
using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.Builder;

/// <summary>
/// Extension methods for performing configuration using <see cref="IConfigurationBuilder"/>
/// in an ASP.NET Core app.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Configures use of Azure App Configuration for feature flags only.
    ///
    /// Expects <see cref="AzureAppConfigurationOptions"/> has been configured in <see cref="AzureAppConfigurationOptions.SectionName"/>.
    /// </summary>
    public static IConfigurationBuilder AddAzureAppConfigurationForWebApp(this IConfigurationBuilder configBuilder, IConfiguration configuration, TokenCredential? azureCredential = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var appConfigurationOptions = configuration
            .GetRequiredSection(AzureAppConfigurationOptions.SectionName)
            .Get<AzureAppConfigurationOptions>();

        if (appConfigurationOptions == null)
            throw new InvalidOperationException("Missing Azure App Configuration.");

        configBuilder.AddAzureAppConfiguration(options =>
        {
            options
                .Connect(new Uri(appConfigurationOptions.Endpoint), azureCredential ?? new DefaultAzureCredential())
                // Using dummy key "_" to avoid loading other configuration than feature flags
                .Select("_")
                // Load all feature flags with no label.
                // Configure the refresh interval according to settings.
                .UseFeatureFlags(options =>
                    options.SetRefreshInterval(TimeSpan.FromSeconds(appConfigurationOptions.FeatureFlagsRefreshIntervalInSeconds)));
        });

        return configBuilder;
    }
}
