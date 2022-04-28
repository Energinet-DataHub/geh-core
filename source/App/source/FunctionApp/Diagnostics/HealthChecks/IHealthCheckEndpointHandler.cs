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

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks
{
    /// <summary>
    /// Handler to support health checks endpoint in a function app.
    /// </summary>
    public interface IHealthCheckEndpointHandler
    {
        /// <summary>
        /// Handle health checks based on <paramref name="endpoint"/>.
        /// </summary>
        /// <param name="httpRequest">Incoming health check request.</param>
        /// <param name="endpoint">Incoming health check endpoint. Can be "live" for liveness check, or "ready" for readiness check.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<HttpResponseData> HandleAsync(HttpRequestData httpRequest, string endpoint);
    }
}
