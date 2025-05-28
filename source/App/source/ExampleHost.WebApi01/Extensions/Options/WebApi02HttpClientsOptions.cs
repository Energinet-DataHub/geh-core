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

namespace ExampleHost.WebApi01.Extensions.Options;

/// <summary>
/// Options for the configuration of ExampleHost.WebApi02 HTTP client.
/// </summary>
public class WebApi02HttpClientsOptions
{
    public const string SectionName = "WebApi02HttpClient";

    /// <summary>
    /// Uri (scope) for which the client must request a token and send as part of the http request.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ApplicationIdUri { get; set; } = string.Empty;

    /// <summary>
    /// Address to the Api hosted in ExampleHost.WebApi02.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ApiBaseAddress { get; set; } = string.Empty;
}
