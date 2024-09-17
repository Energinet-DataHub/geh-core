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

using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Core.Messaging.Communication.Subscriber;

/// <summary>
/// Handler to support dead-letter queue handling in a function app.
/// </summary>
public class DeadLetterHandler
{
    /// <summary>
    /// Suffix added to name of the original entity path (queue or subscription),
    /// to get the full entity path to the dead-letter queue.
    /// </summary>
    public const string DeadLetterQueueSuffix = "/$DeadLetterQueue";

    /// <summary>
    /// Property set to true on ServiceBus message if message is logged by handler.
    /// </summary>
    public const string DeadLetterIsLoggedProperty = "DeadLetterIsLogged";

    public DeadLetterHandler()
    {
    }

    public virtual async Task HandleAsync(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        message.ApplicationProperties.TryGetValue(DeadLetterIsLoggedProperty, out var isLogged);
        if (isLogged is null or (object)false)
        {
            // TODO: Log message HERE
            var propertiesToModify = new Dictionary<string, object>
            {
                [DeadLetterIsLoggedProperty] = true,
            };
            await messageActions.DeferMessageAsync(message, propertiesToModify).ConfigureAwait(false);
        }
    }
}
