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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Client;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Constants;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.ResponseParsers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        [Obsolete("Use 'AddDatabricksSqlStatementExecution'")]
        public static IServiceCollection AddDatabricks(
            this IServiceCollection serviceCollection,
            string warehouseId,
            string workspaceToken,
            string workspaceUrl)
        {
            serviceCollection.AddOptions<DatabricksSqlStatementOptions>().Configure(options =>
            {
                options.WarehouseId = warehouseId;
                options.WorkspaceToken = workspaceToken;
                options.WorkspaceUrl = workspaceUrl;
                options.DatabricksHealthCheckStartHour = 6; // use default
                options.DatabricksHealthCheckEndHour = 20; // use default
            });

            return serviceCollection.AddSqlStatementExecutionInner();
        }

        /// <summary>
        /// Adds the <see cref="IDatabricksSqlStatementClient"/> and related services to the service collection.
        /// </summary>
        /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
        public static IServiceCollection AddDatabricksSqlStatementExecution(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection
                .AddOptions<DatabricksSqlStatementOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .Validate(
                    options =>
                    {
                        return options.DatabricksHealthCheckStartHour < options.DatabricksHealthCheckEndHour;
                    },
                    "Databricks Jobs Health Check end hour must be greater than start hour.");

            return serviceCollection.AddSqlStatementExecutionInner();
        }

        private static IServiceCollection AddSqlStatementExecutionInner(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient(
                HttpClientNameConstants.Databricks,
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<DatabricksSqlStatementOptions>>().Value;

                    client.BaseAddress = new Uri(options.WorkspaceUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.WorkspaceToken);
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
