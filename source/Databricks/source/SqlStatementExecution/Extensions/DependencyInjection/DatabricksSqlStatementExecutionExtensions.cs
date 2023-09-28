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

using System;
using System.Net.Http.Headers;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Extensions.DependencyInjection
{
    public static class DatabricksSqlStatementExecutionExtensions
    {
        /// <summary>
        /// Adds the <see cref="IDatabricksSqlStatementClient"/> and related services to the service collection.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="warehouseId"></param>
        /// <param name="workspaceToken"></param>
        /// <param name="workspaceUrl"></param>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        [Obsolete("Use 'AddSqlStatementExecution'")]
        public static IServiceCollection AddDatabricks(
            this IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
        {
            return AddSqlStatementExecutionInner(serviceCollection, warehouseId, workspaceToken, workspaceUrl);
        }

        /// <summary>
        /// Adds the <see cref="IDatabricksSqlStatementClient"/> and related services to the service collection.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="databricksOptions"></param>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        public static IServiceCollection AddDatabricksSqlStatementExecution(
            this IServiceCollection serviceCollection,
            DatabricksSqlStatementOptions databricksOptions)
        {
            return AddSqlStatementExecutionInner(
                serviceCollection,
                databricksOptions.WarehouseId,
                databricksOptions.WorkspaceToken,
                databricksOptions.WorkspaceUrl,
                databricksOptions.DatabricksHealthCheckStartHour,
                databricksOptions.DatabricksHealthCheckEndHour);
        }

        /// <summary>
        /// Adds Databricks SqlStatementExecution to the service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="warehouseId"></param>
        /// <param name="workspaceToken"></param>
        /// <param name="workspaceUrl"></param>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        public static IServiceCollection AddDatabricksSqlStatementExecution(
            this IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
        {
            return AddSqlStatementExecutionInner(serviceCollection, warehouseId, workspaceToken, workspaceUrl);
        }

        private static IServiceCollection AddSqlStatementExecutionInner(
            IServiceCollection serviceCollection,
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

            serviceCollection.AddHttpClient(
                HttpClientNameConstants.Databricks,
                client =>
                {
                    client.BaseAddress = new Uri(workspaceUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", workspaceToken);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                });

            serviceCollection.AddScoped<IDatabricksSqlStatementClient, DatabricksSqlStatementClient>();
            serviceCollection.AddScoped<ISqlResponseParser, SqlResponseParser>();
            serviceCollection.AddScoped<ISqlStatusResponseParser, SqlStatusResponseParser>();
            serviceCollection.AddScoped<ISqlChunkResponseParser, SqlChunkResponseParser>();
            serviceCollection.AddScoped<ISqlChunkDataResponseParser, SqlChunkDataResponseParser>();

            return serviceCollection;
        }
    }
}
