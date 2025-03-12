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
    /// The <see cref="WorkspaceTokenProvider"/> is used to authenticate requests.
    /// </summary>
    /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
    public static IServiceCollection AddDatabricksSqlStatementExecution(this IServiceCollection serviceCollection, IConfiguration configuration)
        => AddDatabricksSqlStatementExecution(serviceCollection, configuration, TokenProvider.WorkspaceTokenProvider);

    /// <summary>
    /// Adds the <see cref="DatabricksSqlWarehouseQueryExecutor"/> and related services to the service collection.
    /// </summary>
    /// <returns>IServiceCollection containing elements needed to request Databricks SQL Statement Execution API</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <see cref="TokenProvider"/> is not a known value</exception>
    public static IServiceCollection AddDatabricksSqlStatementExecution(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        TokenProvider tokenProvider)
    {
        Action<IServiceCollection> config = tokenProvider switch {
            TokenProvider.WorkspaceTokenProvider => s => s.AddSingleton<ITokenProvider, WorkspaceTokenProvider>(),
            TokenProvider.ServicePrincipalTokenProvider => s => s.AddSingleton<ITokenProvider, ServicePrincipalTokenProvider>(),
            TokenProvider.AzureCliTokenProvider => s => s.AddSingleton<ITokenProvider, AzureCliTokenProvider>(),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenProvider), tokenProvider, null),
        };

        config(serviceCollection);

        return ConfigureDatabricksSqlStatementExecutionDependencies(serviceCollection, configuration);
    }

    private static IServiceCollection ConfigureDatabricksSqlStatementExecutionDependencies(
        IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddOptions<DatabricksSqlStatementOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        serviceCollection.AddTransient<AuthenticateRequestWithToken>();
        serviceCollection
            .AddHttpClient(
                HttpClientNameConstants.Databricks,
                (serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<DatabricksSqlStatementOptions>>().Value;

                    client.BaseAddress = new Uri(options.WorkspaceUrl);
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

public enum TokenProvider
{
    /// <summary>
    /// This is the legacy token provider. Reading a token from configuration.
    /// </summary>
    WorkspaceTokenProvider,

    /// <summary>
    /// Using a service principal to authenticate requests.
    /// </summary>
    ServicePrincipalTokenProvider,

    /// <summary>
    /// Using Azure CLI to authenticate requests when running integration tests.
    /// </summary>
    AzureCliTokenProvider,
}
