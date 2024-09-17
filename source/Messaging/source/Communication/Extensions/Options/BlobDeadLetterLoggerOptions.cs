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

namespace Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;

/// <summary>
/// Options for ServiceBus dead-letter logging in the DH3 system.
/// </summary>
public class BlobDeadLetterLoggerOptions
{
    public const string SectionName = "DeadLetterLogging";

    [Required]
    public string StorageAccountUrl { get; set; } = string.Empty;

    /// <summary>
    /// See container name constraints here: https://learn.microsoft.com/en-us/rest/api/storageservices/Naming-and-Referencing-Containers--Blobs--and-Metadata#container-names
    /// </summary>
    [Required]
    public string ContainerName { get; set; } = "dead-letter-logs";
}
