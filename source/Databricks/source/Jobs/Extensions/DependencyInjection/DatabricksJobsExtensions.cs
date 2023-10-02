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

using Energinet.DataHub.Core.Databricks.Jobs.Abstractions;
using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Energinet.DataHub.Core.Databricks.Jobs.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.Jobs.Extensions.DependencyInjection
{
    public static class DatabricksJobsExtensions
    {
        /// <summary>
        /// Adds Databricks Jobs Client to the service collection.
        /// </summary>
        /// <returns>IServiceCollection containing elements needed to request Databricks Jobs API.</returns>
        public static IServiceCollection AddDatabricksJobs(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IJobsApiClient, JobsApiClient>();
            serviceCollection
                .AddOptions<DatabricksJobsOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .Validate(
                    options =>
                    {
                        return options.DatabricksHealthCheckStartHour < options.DatabricksHealthCheckEndHour;
                    },
                    "Databricks Jobs Health Check end hour must be greater than start hour.");

            return serviceCollection;
        }
    }
}
