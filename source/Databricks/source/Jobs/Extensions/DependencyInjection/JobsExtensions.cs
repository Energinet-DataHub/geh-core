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

using Energinet.DataHub.Core.Databricks.Jobs.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection
{
    public static class JobsExtensions
    {
        /// <summary>
        /// Adds Databricks Jobs Client to the service collection.
        /// </summary>
        /// <returns>IServiceCollection containing elements needed to request Databricks Jobs API.</returns>
        public static IServiceCollection AddDatabricksJobs(this IServiceCollection serviceCollection)
        {
            return AddJobInner(serviceCollection);
        }

        private static IServiceCollection AddJobInner(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IJobsApiClient, JobsApiClient>();
            return serviceCollection;
        }
    }
}
