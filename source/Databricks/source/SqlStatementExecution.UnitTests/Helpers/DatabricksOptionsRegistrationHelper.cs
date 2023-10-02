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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Helpers;

public static class DatabricksOptionsRegistrationHelper
{
    public static void AddDataBricksOptionsToServiceCollection(
        this IServiceCollection serviceCollection,
        string warehouseId,
        string workspaceToken,
        string workspaceUrl,
        int healthCheckStartHour = 6,
        int healthCheckEndHour = 20)
    {
        serviceCollection.AddOptions<DatabricksSqlStatementOptions>().Configure(options =>
        {
            options.WarehouseId = warehouseId;
            options.WorkspaceToken = workspaceToken;
            options.WorkspaceUrl = workspaceUrl;
            options.DatabricksHealthCheckStartHour = healthCheckStartHour;
            options.DatabricksHealthCheckEndHour = healthCheckEndHour;
        });
    }
}
