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
using System.Collections.Generic;
using System.Net.Http;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks.ServiceLiveEndpointHealthCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks
{
    /// <summary>
    /// Provides extension methods for registering DataHub relevant health checks with the <see cref="IHealthChecksBuilder"/>.
    /// </summary>
    public static class HealthChecksBuilderExtensions
    {
        /// <summary>
        /// Adds the health check to be used by the "live" endpoint.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/> for chaining.</returns>
        public static IHealthChecksBuilder AddLiveCheck(this IHealthChecksBuilder builder)
        {
            return builder.AddCheck(HealthChecksConstants.LiveHealthCheckName, () => HealthCheckResult.Healthy());
        }

        public static IHealthChecksBuilder AddServiceLiveHealthCheck(
            this IHealthChecksBuilder builder,
            string serviceName,
            Uri serviceUri,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name: serviceName,
                factory: sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    return new ServiceLiveHealthCheck(serviceUri, () => httpClientFactory.CreateClient(serviceName));
                },
                failureStatus: failureStatus,
                tags: tags,
                timeout: timeout));
        }
    }
}
