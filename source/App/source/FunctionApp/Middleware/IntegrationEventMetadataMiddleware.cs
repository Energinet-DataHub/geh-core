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
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.IntegrationEventContext;
using Energinet.DataHub.Core.App.FunctionApp.Extensions;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.Helpers;
using Energinet.DataHub.Core.JsonSerialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware
{
    public sealed class IntegrationEventMetadataMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IIntegrationEventContext _integrationEventContext;
        private readonly IntegrationEventMetadataParser _integrationEventMetadataParser;

        public IntegrationEventMetadataMiddleware(
            IJsonSerializer jsonSerializer,
            IIntegrationEventContext integrationEventContext)
        {
            _integrationEventContext = integrationEventContext;
            _integrationEventMetadataParser = new IntegrationEventMetadataParser(jsonSerializer);
        }

        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Is(TriggerType.ServiceBusTrigger))
            {
                return next(context);
            }

            if (_integrationEventMetadataParser.TryParse(context, out var userProperties))
            {
                _integrationEventContext.SetMetadata(
                    userProperties.MessageType,
                    userProperties.OperationTimestamp,
                    userProperties.OperationCorrelationId);
            }

            return next(context);
        }
    }
}
