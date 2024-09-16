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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using ExampleHost.FunctionApp.IntegrationEvents.Contracts;

namespace ExampleHost.FunctionApp.IntegrationEvents;

/// <summary>
/// When sending ServiceBus messages from tests we use the event content to trigger a given test scenario.
/// </summary>
public sealed class ExampleIntegrationEventHandler : IIntegrationEventHandler
{
    public Task HandleAsync(IntegrationEvent integrationEvent)
    {
        switch (integrationEvent.Message)
        {
            case TokenV1 tokenV1:
                if (tokenV1.Content == "DeadLetter")
                {
                    // Scenario: Throw exception for e.g. testing failure during event processing
                    throw new InvalidOperationException("Content contains 'DeadLetter'.");
                }

                // Scenario: Successful event processing
                return Task.CompletedTask;
            default:
                // Not used
                throw new InvalidOperationException("Integration Event type not supported.");
        }
    }
}
