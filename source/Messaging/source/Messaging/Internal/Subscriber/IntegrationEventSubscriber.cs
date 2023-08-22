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

using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

internal sealed class IntegrationEventSubscriber : IIntegrationEventSubscriber
{
    private readonly IOptions<SubscriberWorkerOptions> _options;
    private readonly IServiceBusReceiverProvider _serviceBusReceiverProvider;
    private readonly ISubscriber _subscriber;
    private readonly ILogger<IntegrationEventSubscriber> _logger;

    public IntegrationEventSubscriber(
        IOptions<SubscriberWorkerOptions> options,
        IServiceBusReceiverProvider serviceBusReceiverProvider,
        ISubscriber subscriber,
        ILogger<IntegrationEventSubscriber> logger)
    {
        _options = options;
        _serviceBusReceiverProvider = serviceBusReceiverProvider;
        _subscriber = subscriber;
        _logger = logger;
    }

    public async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        foreach (var message in await _serviceBusReceiverProvider.Instance.ReceiveMessagesAsync(_options.Value.MaxMessageDeliveryCount, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var rawServiceBusMessage = new IntegrationEventServiceBusMessage(
                    Guid.Parse(message.MessageId),
                    message.Subject,
                    message.ApplicationProperties,
                    new BinaryData(message.Body.ToArray()));

                await _subscriber.HandleAsync(rawServiceBusMessage).ConfigureAwait(false);
                await _serviceBusReceiverProvider.Instance.CompleteMessageAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // TODO: Retry logic
                await _serviceBusReceiverProvider.Instance.DeadLetterMessageAsync(message, cancellationToken: cancellationToken).ConfigureAwait(false);
                _logger.LogError(e, "Failed to process message {MessageId}", message.MessageId);
            }
        }
    }
}
