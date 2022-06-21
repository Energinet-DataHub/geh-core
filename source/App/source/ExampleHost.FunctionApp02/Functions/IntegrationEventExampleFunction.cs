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

using ExampleHost.FunctionApp02.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ExampleHost.FunctionApp02.Functions
{
    public class IntegrationEventExampleFunction
    {
        private readonly ILogger _logger;

        public IntegrationEventExampleFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IntegrationEventExampleFunction>();
        }

        [Function(nameof(ReceiveMessage))]
        public void ReceiveMessage(
            [ServiceBusTrigger(
                EnvironmentSettingNames.IntegrationEventTopicName,
                EnvironmentSettingNames.IntegrationEventSubscriptionName,
                Connection = EnvironmentSettingNames.IntegrationEventConnectionString)]
            string serviceBusMessage)
        {
            _logger.LogInformation($"{nameof(ReceiveMessage)}: We should be able to find this log message by following the trace of the request.");
        }
    }
}
