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

using ExampleHost.FunctionApp01.Functions;

namespace ExampleHost.FunctionApp01.FeatureManagement;

/// <summary>
/// Names of Feature Flags that exists in subsystem.
///
/// The feature flags can be configured:
///  * Using App Settings (locally or in Azure)
///  * In Azure App Configuration
///
/// If configured using App Settings, the name of a feature flag
/// configuration must be prefixed with <see cref="SectionName"/>,
/// ie. "FeatureManagement__UseGetMessage".
/// </summary>
/// <remarks>
/// We use "const" for feature flags instead of a enum, because "Produkt Mål's"
/// feature flags contain "-" in their name.
/// </remarks>
public static class FeatureFlagNames
{
    /// <summary>
    /// Configuration section name when configuring feature flags as App Settings.
    /// </summary>
    public const string SectionName = "FeatureManagement";

    /// <summary>
    /// Whether to use demo function <see cref="FeatureManagementFunction.GetMessage(Microsoft.Azure.Functions.Worker.Http.HttpRequestData)"/>.
    /// </summary>
    public const string UseGetMessage = "UseGetMessage";

    // Add additional feature flag names here...
}
