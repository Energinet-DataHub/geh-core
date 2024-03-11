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

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;

/// <summary>
/// Settings necessary for managing some Azure resources, like Event Hub, in the Integration Test environment.
/// </summary>
public class AzureResourceManagementSettings
{
    public string TenantId { get; internal set; }
        = string.Empty;

    public string SubscriptionId { get; internal set; }
        = string.Empty;

    public string ResourceGroup { get; internal set; }
        = string.Empty;

    public string ClientId { get; internal set; }
        = string.Empty;

    public string ClientSecret { get; internal set; }
        = string.Empty;
}
