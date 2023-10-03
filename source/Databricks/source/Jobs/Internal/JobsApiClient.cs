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
using Energinet.DataHub.Core.Databricks.Jobs.Internal.Constants;
using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Core.Databricks.Jobs.Internal
{
    /// <summary>
    /// A databricks client based on the Microsoft.Azure.JobsApiClient, which is using Job API 2.0.
    /// The client is extended with a method for reading jobs created using Python Wheels, using Job API 2.1.
    /// Because the Job API 2.0 does not support reading python wheel settings.
    /// Which is used when we run new jobs and need to know the existing parameters of the job.
    /// The code is based on https://github.com/Azure/azure-databricks-client and can be replaced by the official
    /// package when support for Job API 2.1 is added.
    /// </summary>
    public sealed class JobsApiClient : IDisposable, IJobsApiClient
    {
        /// <summary>
        /// Create Databricks Jobs client object with a Http client.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/>.</param>
        public JobsApiClient(IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient(HttpClientNameConstants.DatabricksJobsApi);
            Jobs = new Microsoft.Azure.Databricks.Client.JobsApiClient(httpClient);
        }

        public IJobsApi Jobs { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Jobs.Dispose();
            }
        }
    }
}
