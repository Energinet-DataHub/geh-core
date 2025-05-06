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

namespace Energinet.DataHub.Core.App.Common.Extensions.Options;

/// <summary>
/// Options for the configuration of Azure App Configuration.
/// </summary>
public class AzureAppConfigurationOptions
{
    public const string SectionName = "AzureAppConfiguration";

    /// <summary>
    /// The endpoint of the Azure App Configuration.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The refresh interval used for feature flags.
    /// Microsoft default for this is 30 seconds.
    /// </summary>
    /// <remarks>
    /// We support the configuration of this to be able to override the default in tests.
    /// In production we should use the default, unless we learn something new (in which
    /// case we should consider changing the default).
    /// </remarks>
    public long FeatureFlagsRefreshIntervalInSeconds { get; set; } = 30;
}
