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

using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Publisher;

internal sealed class IntegrationEventsPublisher : IPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly IIntegrationEventProvider _integrationEventProvider;
    private readonly IServiceBusMessageFactory _serviceBusMessageFactory;
    private readonly ILogger _logger;

    public IntegrationEventsPublisher(
        IOptions<IntegrationEventsOptions> integrationEventsOptions,
        IAzureClientFactory<ServiceBusSender> senderFactory,
        IIntegrationEventProvider integrationEventProvider,
        IServiceBusMessageFactory serviceBusMessageFactory,
        ILogger<IntegrationEventsPublisher> logger)
    {
        _sender = senderFactory.CreateClient(integrationEventsOptions.Value.TopicName);
        _integrationEventProvider = integrationEventProvider;
        _serviceBusMessageFactory = serviceBusMessageFactory;
        _logger = logger;
    }

    public async Task PublishAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var eventCount = 0;
        var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var @event in _integrationEventProvider.GetAsync().WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            eventCount++;
            var serviceBusMessage = _serviceBusMessageFactory.Create(@event);
            if (!messageBatch.TryAddMessage(serviceBusMessage))
            {
                await SendBatchAsync(messageBatch).ConfigureAwait(false);
                messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

                if (!messageBatch.TryAddMessage(serviceBusMessage))
                {
                    await SendMessageThatExceedsBatchLimitAsync(serviceBusMessage).ConfigureAwait(false);
                }
            }
        }

        try
        {
            await _sender.SendMessagesAsync(messageBatch, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish messages");
        }

        if (eventCount > 0)
        {
            _logger.LogDebug("Sent {EventCount} integration events in {Time} ms", eventCount, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private Task SendBatchAsync(ServiceBusMessageBatch batch)
    {
        return _sender.SendMessagesAsync(batch);
    }

    private Task SendMessageThatExceedsBatchLimitAsync(ServiceBusMessage serviceBusMessage)
    {
        return _sender.SendMessageAsync(serviceBusMessage);
    }
}
