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

using System.ComponentModel.DataAnnotations;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.Options;

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// The address of MitID configuration endpoint for the external token.
    /// </summary>
    public string MitIdExternalMetadataAddress { get; set; } = string.Empty;

    /// <summary>
    /// The address of OpenId configuration endpoint for the external token, e.g. https://{b2clogin.com/tenant-id/policy}/v2.0/.well-known/openid-configuration.
    /// </summary>
    public string ExternalMetadataAddress { get; set; } = string.Empty;

    public string BackendBffAppId { get; set; } = string.Empty;

    /// <summary>
    /// The address of OpenId configuration endpoint for the internal token, e.g. https://{market-participant-web-api}/.well-known/openid-configuration.
    /// </summary>
    public string InternalMetadataAddress { get; set; } = string.Empty;
}
