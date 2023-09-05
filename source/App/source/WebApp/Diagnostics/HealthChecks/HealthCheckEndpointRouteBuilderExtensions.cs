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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add DataHub relevant health checks endpoints.
    /// </summary>
    public static class HealthCheckEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Adds the "live" health checks endpoint to the <see cref="IEndpointRouteBuilder"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the health checks endpoint to.</param>
        /// <returns>A convention routes for the health checks endpoint.</returns>
        public static IEndpointConventionBuilder MapLiveHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            // Health check endpoints must allow anonymous access so we can use them with Azure monitoring:
            // https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check#authentication-and-security
            return endpoints
                .MapHealthChecks(
                    HealthChecksConstants.LiveHealthCheckEndpointRoute,
                    new HealthCheckOptions
                    {
                        Predicate = r => r.Name.Equals(HealthChecksConstants.LiveHealthCheckName),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    })
                .WithMetadata(new AllowAnonymousAttribute());
        }

        /// <summary>
        /// Adds the "ready" health checks endpoint to the <see cref="IEndpointRouteBuilder"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the health checks endpoint to.</param>
        /// <returns>A convention routes for the health checks endpoint.</returns>
        public static IEndpointConventionBuilder MapReadyHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            // Health check endpoints must allow anonymous access so we can use them with Azure monitoring:
            // https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check#authentication-and-security
            return endpoints
                .MapHealthChecks(
                    HealthChecksConstants.ReadyHealthCheckEndpointRoute,
                    new HealthCheckOptions
                    {
                        Predicate = r => !r.Name.Equals(HealthChecksConstants.LiveHealthCheckName),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    })
                .WithMetadata(new AllowAnonymousAttribute());
        }
    }
}
