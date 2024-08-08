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
/// Settings necessary for managing the Azure AD B2C located in the integration test environment.
/// </summary>
public record AzureB2CSettings()
{
    public string Tenant { get; internal set; }
        = string.Empty;

    public string ServicePrincipalId { get; internal set; }
        = string.Empty;

    public string ServicePrincipalSecret { get; internal set; }
        = string.Empty;

    public string BackendAppId { get; internal set; }
        = string.Empty;

    public string BackendServicePrincipalObjectId { get; internal set; }
        = string.Empty;

    public string BackendAppObjectId { get; internal set; }
        = string.Empty;

    /// <summary>
    /// This is not the actual BFF but a test app registration that allows
    /// us to verify some of the JWT code.
    /// </summary>
    public string TestBffAppId { get; internal set; }
        = string.Empty;
}
