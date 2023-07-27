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
using Energinet.DataHub.Core.SqlStatement.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.SqlStatement.Extensions
{
    public static class DatabricksExtensions
    {
        public static IServiceCollection AddDatabricks(
            this IServiceCollection services,
            DatabricksOptions databricksOptions)
        {
            services.AddSingleton(databricksOptions);
            services.AddScoped<ISqlStatementClient, SqlStatementClient>();
            services.AddScoped<IJsonSerializer, JsonSerializer>();

            services.AddHttpClient("DatabricksStatementExecutionApi", client =>
            {
                client.BaseAddress = CreateUri(databricksOptions);
                client.DefaultRequestHeaders.Add(
                    "Authorization",
                    "Bearer " + databricksOptions.ClusterAccessToken);
            });

            return services;
        }

        private static Uri CreateUri(DatabricksOptions databricksOptions)
        {
            return new Uri("https://" +
                           databricksOptions.Instance +
                           databricksOptions.Endpoint +
                           databricksOptions.WarehouseId);
        }
    }
}
