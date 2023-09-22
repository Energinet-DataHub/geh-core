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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.AppSettings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Diagnostics.HealthChecks;

public class DatabricksSqlStatementApiHealthRegistration : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClock _clock;
    private readonly DatabricksSqlStatementOptions _options;

    public DatabricksSqlStatementApiHealthRegistration(IHttpClientFactory httpClientFactory, IClock clock, DatabricksSqlStatementOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _clock = clock;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var currentHour = _clock.GetCurrentInstant().ToDateTimeUtc().Hour;
        if (_options.DatabricksHealthCheckStartHour.Hour <= currentHour && currentHour <= _options.DatabricksHealthCheckEndHour.Hour)
        {
            var httpClient = CreateHttpClient();
            var url = $"{_options.WorkspaceUrl}/api/2.0/sql/warehouses/{_options.WarehouseId}";
            var response = await httpClient
                .GetAsync(url, cancellationToken)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }

        return HealthCheckResult.Healthy();
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.WorkspaceUrl);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.WorkspaceToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.BaseAddress = new Uri(_options.WorkspaceUrl);
        return httpClient;
    }
}
