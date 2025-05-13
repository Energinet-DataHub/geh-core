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
using Microsoft.FeatureManagement;

namespace ExampleHost.FunctionApp01.FeatureManagement;

/// <summary>
/// Extensions for reading feature flags in subsystem.
/// The extension methods allows us to create an abstraction from the feature flag names,
/// explaining the meaning of the feature flag in the subsystem. This is especially relevant for
/// "Produkt Mål's" feature flags, as they might be used in multiple subsystems, but
/// mean something different in each subsystem.
/// </summary>
public static class FeatureManagerExtensions
{
    /// <summary>
    /// Whether to allow the use of the demo function <see cref="FeatureManagementFunction.GetMessage(Microsoft.Azure.Functions.Worker.Http.HttpRequestData)"/>.
    /// </summary>
    public static Task<bool> UseGetMessagesAsync(this IFeatureManager featureManager)
    {
        return featureManager.IsEnabledAsync(FeatureFlagNames.UseGetMessage);
    }

    // Add extension methods for each feature flag name here...
}
