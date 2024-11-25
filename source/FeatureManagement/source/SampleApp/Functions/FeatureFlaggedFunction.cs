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

using System.Net;
using Energinet.DataHub.Core.FeatureManagement.SampleApp.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.FeatureManagement;

namespace Energinet.DataHub.Core.FeatureManagement.SampleApp.Functions
{
    public class FeatureFlaggedFunction
    {
        public FeatureFlaggedFunction(IFeatureManager featureManager)
        {
            FeatureManager = featureManager;
        }

        private IFeatureManager FeatureManager { get; }

        /// <summary>
        /// Demonstrate how we can use FeatureManager to switch on a feature flag.
        ///
        /// See the integration tests for this method for more on how it works, and how it can be tested.
        /// </summary>
        [Function(nameof(GetMessageAsync))]
        public async Task<HttpResponseData> GetMessageAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "message")]
            HttpRequestData request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            // If the feature is enabled, we return a Guid instead of the static message.
            var isFeatureEnabled = await FeatureManager.IsEnabledAsync(nameof(FeatureFlags.Names.UseGuidMessage));
            if (isFeatureEnabled)
            {
                response.WriteString(Guid.NewGuid().ToString());
                return response;
            }

            response.WriteString("Static message text");
            return response;
        }

        /// <summary>
        /// Demonstrate how we can use an app setting (see local.settings.json) to disable an Azure Function.
        /// See also: https://docs.microsoft.com/en-us/azure/azure-functions/disable-function
        ///
        /// See the integration tests for this method for more on how it works, and how it can be tested.
        /// </summary>
        [Function(nameof(CreateMessage))]
        public HttpResponseData CreateMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "message")]
            HttpRequestData request)
        {
            return request.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
