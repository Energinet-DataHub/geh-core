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

using Energinet.DataHub.Core.App.Common.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding HealthChecks services to a Function App.
/// </summary>
public static class HealthChecksExtensions
{
    /// <summary>
    /// This operation is idempotent.
    ///
    /// Register services necessary for using Health Checks in a Function App.
    /// </summary>
    public static IServiceCollection AddHealthChecksForIsolatedWorker(this IServiceCollection services)
    {
        if (!IsHealthChecksAdded(services))
        {
            services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
            services.AddHealthChecks()
                .AddLiveCheck();
        }

        return services;
    }

    private static bool IsHealthChecksAdded(IServiceCollection services)
    {
        return services.Any((service) => service.ServiceType == typeof(IHealthCheckEndpointHandler));
    }
}
