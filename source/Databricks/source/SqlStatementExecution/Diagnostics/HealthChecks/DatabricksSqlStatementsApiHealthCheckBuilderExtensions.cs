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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Diagnostics.HealthChecks;

public static class DatabricksSqlStatementsApiHealthCheckBuilderExtensions
{
    private const string Name = "DatabricksSqlStatementApiHealthCheck";

    /// <summary>
    /// Add a health check of the Databricks SQL Statement Execution Api to be used by the "ready" endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="name">The name of the service to call.</param>
    /// <param name="failureStatus">The response health status on failure.</param>
    /// <param name="tags">A list of tags that can be used for filtering health checks.</param>
    /// <param name="timeout">The amount of time to wait before timing out.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> for chaining.</returns>
    public static IHealthChecksBuilder AddDatabricksSqlStatementApiHealthCheck(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? Name,
            serviceProvider => new DatabricksSqlStatementApiHealthCheck(
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                serviceProvider.GetRequiredService<IOptions<DatabricksSqlStatementOptions>>()),
            failureStatus,
            tags,
            timeout));
    }
}
