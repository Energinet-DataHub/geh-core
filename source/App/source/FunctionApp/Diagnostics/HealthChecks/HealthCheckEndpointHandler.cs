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
using System.Net;
using System.Threading.Tasks;
using Azure;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks
{
    public class HealthCheckEndpointHandler : IHealthCheckEndpointHandler
    {
        public HealthCheckEndpointHandler(HealthCheckService healthCheckService)
        {
            HealthCheckService = healthCheckService;
        }

        private HealthCheckService HealthCheckService { get; }

        public async Task<HttpResponseData> HandleAsync(HttpRequestData httpRequest, string endpoint)
        {
            Func<HealthCheckRegistration, bool>? predicate = null;
            if (string.Compare(endpoint, "live", ignoreCase: true) == 0)
            {
                predicate = r => r.Name.Contains(HealthChecksConstants.LiveHealthCheckName);
            }

            if (string.Compare(endpoint, "ready", ignoreCase: true) == 0)
            {
                predicate = r => !r.Name.Contains(HealthChecksConstants.LiveHealthCheckName);
            }

            var httpResponse = httpRequest.CreateResponse();

            if (predicate == null)
            {
                httpResponse.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                var result = await HealthCheckService.CheckHealthAsync(predicate).ConfigureAwait(false);

                httpResponse.StatusCode = result.Status == HealthStatus.Healthy
                    ? HttpStatusCode.OK
                    : HttpStatusCode.ServiceUnavailable;

                httpResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                var healthStatus = Enum.GetName(typeof(HealthStatus), result.Status);
                await httpResponse.WriteStringAsync(healthStatus!).ConfigureAwait(false);
            }

            return httpResponse;
        }
    }
}
