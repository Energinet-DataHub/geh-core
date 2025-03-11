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

using System.Net.Http.Headers;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public static class DatabricksSqlStatementExecutionExtensions
{
    /// <summary>
    /// Adds the <see cref="DatabricksSqlWarehouseQueryExecutor"/> and related services to the service collection.
    /// </summary>
    /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
    public static IServiceCollection AddDatabricksSqlStatementExecution(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddOptions<DatabricksSqlStatementOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        serviceCollection
            .AddHttpClient(
                HttpClientNameConstants.Databricks,
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<DatabricksSqlStatementOptions>>().Value;

                    client.BaseAddress = new Uri(options.WorkspaceUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.WorkspaceToken);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                }).AddHttpMessageHandler<AuthenticateRequestWithToken>();

        serviceCollection.AddSingleton(sp =>
            new DatabricksSqlWarehouseQueryExecutor(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<IOptions<DatabricksSqlStatementOptions>>()));

        return serviceCollection;
    }
}
