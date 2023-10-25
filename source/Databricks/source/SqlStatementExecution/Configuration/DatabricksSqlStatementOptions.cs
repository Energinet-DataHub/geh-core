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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;

public class DatabricksSqlStatementOptions
{
    /// <summary>
    /// Settings scope for Databricks options.
    /// </summary>
    public const string DatabricksOptions = "DatabricksOptions";

    /// <summary>
    /// Base URL for the databricks resource. For example: https://southcentralus.azuredatabricks.net.
    /// </summary>
    [Required]
    public string WorkspaceUrl { get; set; } = string.Empty;

    /// <summary>
    /// The access token. To generate a token, refer to this document: https://docs.databricks.com/api/latest/authentication.html#generate-a-token.
    /// </summary>
    [Required]
    public string WorkspaceToken { get; set; } = string.Empty;

    /// <summary>
    /// The databricks warehouse id.
    /// </summary>
    [Required]
    public string WarehouseId { get; set; } = string.Empty;

    /// <summary>
    /// Seconds we allow the Databricks Statement Execution Api to respond.
    /// </summary>
    [Required]
    public int TimeoutInSeconds { get; set; } = 30;

    /// <summary>
    /// Defines the hour of the day when the health check DataLake should start.
    /// The default value is 6:00 AM.
    /// </summary>
    [Range(0, 23, ErrorMessage = "Value for {0} must be between {1} and {2} inclusive.")]
    public int DatabricksHealthCheckStartHour { get; set; } = 6;

    /// <summary>
    /// Defines the hour of the day when the health check towards DataLake should end.
    /// The default value is 8:00 PM.
    /// </summary>
    [Range(0, 23, ErrorMessage = "Value for {0} must be between {1} and {2} inclusive.")]
    public int DatabricksHealthCheckEndHour { get; set; } = 20;
}
