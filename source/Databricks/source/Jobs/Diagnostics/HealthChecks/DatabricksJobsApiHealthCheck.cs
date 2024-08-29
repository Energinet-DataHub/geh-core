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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;

public class DatabricksJobsApiHealthCheck : IHealthCheck
{
    private readonly IJobsApiClient _jobsApiClient;
    private readonly DatabricksJobsOptions _options;

    public DatabricksJobsApiHealthCheck(
        IJobsApiClient jobsApiClient,
        IOptions<DatabricksJobsOptions> options)
    {
        _jobsApiClient = jobsApiClient;
        _options = options.Value;
    }

    /// <summary>
    /// Check health of the Databricks Jobs API.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An async task of <see cref="HealthCheckResult"/></returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            await _jobsApiClient.Jobs
                .ListPageable(cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Databricks Jobs API is unhealthy", ex);
        }
    }
}
