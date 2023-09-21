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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.AppSettings;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Extensions.DependencyInjection
{
    public static class SqlStatementExecutionExtensions
    {
        /// <summary>
        /// Adds Databricks SqlStatementExecution to the service collection
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
            return AddSqlStatementExecutionInner<DatabricksSqlStatementClient>(serviceCollection, warehouseId, workspaceToken, workspaceUrl);
        }

        /// <summary>
        /// Adds Databricks SqlStatementExecution to the service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="databricksOptions"></param>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        public static IServiceCollection AddSqlStatementExecution(
            this IServiceCollection serviceCollection,
            DatabricksOptions databricksOptions)
        {
            return AddSqlStatementExecutionInner<DatabricksSqlStatementClient>(serviceCollection, databricksOptions.WarehouseId, databricksOptions.WorkspaceToken, databricksOptions.WorkspaceUrl);
        }

        /// <summary>
        /// Adds Databricks SqlStatementExecution to the service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="warehouseId"></param>
        /// <param name="workspaceToken"></param>
        /// <param name="workspaceUrl"></param>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        public static IServiceCollection AddSqlStatementExecution(
            this IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
        {
            return AddSqlStatementExecutionInner<DatabricksSqlStatementClient>(serviceCollection, warehouseId, workspaceToken, workspaceUrl);
        }

        /// <summary>
        /// Adds Databricks SqlStatementExecution to the service collection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="warehouseId"></param>
        /// <param name="workspaceToken"></param>
        /// <param name="workspaceUrl"></param>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        internal static IServiceCollection AddSqlStatementExecution<T>(
            this IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
            where T : class, IDatabricksSqlStatementClient
        {
            return AddSqlStatementExecutionInner<T>(serviceCollection, warehouseId, workspaceToken, workspaceUrl);
        }

        private static IServiceCollection AddSqlStatementExecutionInner<T>(
            IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
            where T : class, IDatabricksSqlStatementClient
        {
            serviceCollection.AddOptions<DatabricksOptions>().Configure(options =>
            {
                options.WarehouseId = warehouseId;
                options.WorkspaceToken = workspaceToken;
                options.WorkspaceUrl = workspaceUrl;
            });

            serviceCollection.AddScoped<IDatabricksSqlStatementClient, T>();

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

            serviceCollection.AddScoped<IDatabricksSqlResponseParser, DatabricksSqlResponseParser>();
            serviceCollection.AddScoped<IDatabricksSqlStatusResponseParser, DatabricksSqlStatusResponseParser>();
            serviceCollection.AddScoped<IDatabricksSqlChunkResponseParser, DatabricksSqlChunkResponseParser>();
            serviceCollection.AddScoped<IDatabricksSqlChunkDataResponseParser, DatabricksSqlChunkDataResponseParser>();

            return serviceCollection;
        }
    }
}
