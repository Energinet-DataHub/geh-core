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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Extensions;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.Helpers;
using Energinet.DataHub.Core.JsonSerialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId
{
    public sealed class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly IntegrationEventMetadataParser _integrationEventMetadataParser;
        private readonly IJsonSerializer _jsonSerializer;

        public CorrelationIdMiddleware(
            IJsonSerializer jsonSerializer,
            ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
            _jsonSerializer = jsonSerializer;
            _integrationEventMetadataParser = new IntegrationEventMetadataParser(_jsonSerializer);
        }

        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Is(TriggerType.ServiceBusTrigger))
            {
                if (_integrationEventMetadataParser.TryParse(context, out var userProperties))
                {
                    _correlationContext.SetId(userProperties.OperationCorrelationId);
                }
            }
            else if (context.Is(TriggerType.HttpTrigger))
            {
                context.BindingContext.BindingData.TryGetValue("Headers", out var headersObj);

                if (headersObj is not string headersStr)
                {
                    throw new ArgumentException("Headers was not a string");
                }

                // Deserialize headers from JSON
                var headers = _jsonSerializer.Deserialize<Dictionary<string, string>>(headersStr);

                if (headers == null)
                {
                    throw new ArgumentException("Could not parse Headers as Json");
                }

                var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
                if (!normalizedKeyHeaders.TryGetValue("Correlation-ID", out var correlationIdHeaderValue))
                {
                    // No Common header present
                    throw new ArgumentException("Correlation-ID header was not present");
                }

                _correlationContext.SetId(correlationIdHeaderValue);
            }

            return next(context);
        }
    }
}
