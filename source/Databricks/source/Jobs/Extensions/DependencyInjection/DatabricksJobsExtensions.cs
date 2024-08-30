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

using System.Net;
using System.Net.Http.Headers;
using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Client;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.Jobs.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection;

public static class DatabricksJobsExtensions
{
    /// <summary>
    /// Adds the <see cref="IJobsApiClient"/> and required options to the service collection.
    /// </summary>
    /// <returns>IServiceCollection containing elements needed to request Databricks Jobs API.</returns>
    public static IServiceCollection AddDatabricksJobs(this IServiceCollection serviceCollection, IConfiguration configuration, long timeout = 30)
    {
        serviceCollection.AddSingleton<IJobsApiClient, JobsApiClient>();

        serviceCollection
            .AddOptions<DatabricksJobsOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations();

        serviceCollection
            .AddHttpClient(
                HttpClientNameConstants.DatabricksJobsApi,
                (services, httpClient) =>
                {
                    var options = services.GetRequiredService<IOptions<DatabricksJobsOptions>>().Value;
                    var url = new Uri(new Uri(options.WorkspaceUrl), "api/");
                    httpClient.BaseAddress = url;
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.WorkspaceToken);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                });

        return serviceCollection;
    }
}
