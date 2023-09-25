﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.Core.Databricks.Jobs.AppSettings;

public class DatabricksJobsOptions
{
    /// <summary>
    /// Base URL for the databricks resource. For example: https://southcentralus.azuredatabricks.net.
    /// </summary>
    public string WorkspaceUrl { get; set; } = string.Empty;

    /// <summary>
    /// The access token. To generate a token, refer to this document: https://docs.databricks.com/api/latest/authentication.html#generate-a-token.
    /// </summary>
    public string WorkspaceToken { get; set; } = string.Empty;

    /// <summary>
    /// The databricks warehouse id.
    /// </summary>
    public string WarehouseId { get; set; } = string.Empty;

    /// <summary>
    /// Defines the hour of the day when the health check towards Databricks Jobs API should start.
    /// The default value is 6 AM. Valid values are 0-23.
    /// </summary>
    public int DatabricksHealthCheckStartHour { get; set; } = 6;

    /// <summary>
    /// Defines the hour of the day when the health check towards Databricks Jobs API should end.
    /// The default value is 8 PM. Valid values are 0-23.
    /// </summary>
    public int DatabricksHealthCheckEndHour { get; set; } = 20;
}