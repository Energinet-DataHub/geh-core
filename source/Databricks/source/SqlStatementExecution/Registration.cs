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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.AppSettings;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution
{
    public static class Registration
    {
        public static IServiceCollection AddDatabricks(
            this IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
        {
            serviceCollection.AddOptions<DatabricksOptions>().Configure(options =>
            {
                options.WarehouseId = warehouseId;
                options.WorkspaceToken = workspaceToken;
                options.WorkspaceUrl = workspaceUrl;
            });

            serviceCollection.AddHttpClient<ISqlStatementClient>();
            serviceCollection.AddScoped<ISqlStatementClient, SqlStatementClient>();
            serviceCollection.AddScoped<IDatabricksSqlResponseParser, DatabricksSqlResponseParser>();
            serviceCollection.AddScoped<IDatabricksSqlStatusResponseParser, DatabricksSqlStatusResponseParser>();
            serviceCollection.AddScoped<IDatabricksSqlChunkResponseParser, DatabricksSqlChunkResponseParser>();
            serviceCollection.AddScoped<IDatabricksSqlChunkDataResponseParser, DatabricksSqlChunkDataResponseParser>();

            return serviceCollection;
        }
    }
}
