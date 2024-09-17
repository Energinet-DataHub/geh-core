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
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

/// <inheritdoc cref="IDeadLetterHandler"/>
internal class DeadLetterHandler : IDeadLetterHandler
{
    /// <summary>
    /// Name of the property that is set to <see langword="true"/> on ServiceBus message,
    /// if the message is logged by handler.
    /// </summary>
    public const string DeadLetterIsLoggedProperty = "DeadLetterIsLogged";

    public DeadLetterHandler()
    {
    }

    /// <summary>
    /// If <paramref name="message"/> has not been logged (handled) previously, then:
    /// <list type="bullet">
    /// <item>Log the <paramref name="message"/>, and update <see cref="DeadLetterIsLoggedProperty" /> accordingly.</item>
    /// <item>Update the <paramref name="message"/> as deferred, so it wont retrigger the dead-letter queue trigger.</item>
    /// </list>
    /// </summary>
    public async Task HandleAsync(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        if (HasNotBeenLogged(message))
        {
            // TODO: Log message HERE; use DeadLetterSource
            var propertiesToModify = new Dictionary<string, object>
            {
                [DeadLetterIsLoggedProperty] = true,
            };
            await messageActions.DeferMessageAsync(message, propertiesToModify).ConfigureAwait(false);
        }
    }

    private static bool HasNotBeenLogged(ServiceBusReceivedMessage message)
    {
        message.ApplicationProperties.TryGetValue(DeadLetterIsLoggedProperty, out var isLogged);
        return isLogged is null or (object)false;
    }
}
