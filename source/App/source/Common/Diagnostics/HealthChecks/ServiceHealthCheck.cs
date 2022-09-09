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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks
{
    /// <summary>
    /// The <see cref="IHealthCheck"/> that can be used for calling the health status of other services.
    /// </summary>
    public class ServiceHealthCheck : IHealthCheck
    {
        private readonly Uri _serviceUri;
        private readonly Func<HttpClient> _httpClientFactory;

        public ServiceHealthCheck(Uri serviceUri, Func<HttpClient> httpClientFactory)
        {
            _serviceUri = serviceUri ?? throw new ArgumentNullException(nameof(serviceUri));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory();

                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, _serviceUri);

                using var response = await httpClient.SendAsync(requestMessage, cancellationToken);

                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy()
                    : new HealthCheckResult(context.Registration.FailureStatus);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
