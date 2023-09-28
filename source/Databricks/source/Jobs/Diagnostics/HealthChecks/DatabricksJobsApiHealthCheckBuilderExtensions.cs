﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.Databricks.Jobs.AppSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NodaTime;

namespace Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;

public static class DatabricksJobsApiHealthCheckBuilderExtensions
{
    private const string Name = "DatabricksJobsApiHealthCheck";

    /// <summary>
    /// Add a health check to the "ready" endpoint where the health endpoint of another service can be called.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">The <see cref="DatabricksJobsOptions"/>.</param>
    /// <param name="name">The name of the service to call.</param>
    /// <param name="failureStatus">The response health status on failure.</param>
    /// <param name="tags">A list of tags that can be used for filtering health checks.</param>
    /// <param name="timeout">The amount of time to wait before timing out.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> for chaining.</returns>
    public static IHealthChecksBuilder AddDatabricksJobsApiHealthCheck(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, DatabricksJobsOptions> options,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? Name,
            serviceProvider => new DatabricksJobsApiHealthCheck(
                serviceProvider.GetRequiredService<IJobsApiClient>(),
                serviceProvider.GetRequiredService<IClock>(),
                options(serviceProvider)),
            failureStatus,
            tags,
            timeout));
    }
}
