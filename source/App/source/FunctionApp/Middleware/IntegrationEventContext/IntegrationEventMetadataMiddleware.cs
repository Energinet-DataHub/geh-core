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
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware.IntegrationEventContext
{
    public sealed class IntegrationEventMetadataMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger _logger;
        private readonly IIntegrationEventContext _integrationEventContext;

        public IntegrationEventMetadataMiddleware(
            ILogger logger,
            IIntegrationEventContext integrationEventContext)
        {
            _logger = logger;
            _integrationEventContext = integrationEventContext;
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

            if (TryGetUserProperties(context, out var userProperties))
            {
                _integrationEventContext.SetMetadata(
                    userProperties.MessageType,
                    userProperties.OperationTimeStamp);
            }
            else
            {
                var errorMessage =
                    $"Integration event context could not be set up for invocation: {context.InvocationId}";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            return next(context);
        }

        private static bool TryGetUserProperties(
            FunctionContext functionContext,
            [NotNullWhen(true)]
            out IntegrationEventJsonMetadata? userProperties)
        {
            userProperties = null;

            var bindingData = functionContext.BindingContext.BindingData;
            if (bindingData.TryGetValue("UserProperties", out var userPropertiesObject))
            {
                if (userPropertiesObject is string userProps)
                {
                    var userPropertiesDict = JsonSerializer.Deserialize<IntegrationEventJsonMetadata>(userProps);
                    if (userPropertiesDict != null)
                    {
                        userProperties = userPropertiesDict;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
