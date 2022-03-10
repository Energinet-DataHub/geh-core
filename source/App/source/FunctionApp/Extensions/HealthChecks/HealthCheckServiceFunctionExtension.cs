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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.HealthChecks
{
    /// <summary>
    /// Provides extension methods for registering <see cref="HealthCheckService"/> in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class HealthCheckServiceFunctionExtension
    {
        /// <summary>
        /// Adds the <see cref="HealthCheckService"/> to the container, using the provided delegate to register
        /// health checks.
        /// </summary>
        /// <remarks>
        /// This operation is idempotent - multiple invocations will still only result in a single
        /// <see cref="HealthCheckService"/> instance in the <see cref="IServiceCollection"/>. It can be invoked
        /// multiple times in order to get access to the <see cref="IHealthChecksBuilder"/> in multiple places.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the <see cref="HealthCheckService"/> to.</param>
        /// <returns>An instance of <see cref="IHealthChecksBuilder"/> from which health checks can be registered.</returns>
        public static IHealthChecksBuilder AddFunctionHealthChecks(this IServiceCollection services)
        {
            services.TryAddSingleton<HealthCheckService, DefaultHealthCheckService>();
            return new HealthChecksBuilder(services);
        }
    }
}
