﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.Reflection;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

internal sealed class IntegrationEventFactory : IIntegrationEventFactory
{
    private readonly IEnumerable<MessageDescriptor> _descriptors;

    public IntegrationEventFactory(IEnumerable<MessageDescriptor> descriptors)
    {
        _descriptors = descriptors;
    }

    public IntegrationEvent Create(IntegrationEventServiceBusMessage message)
    {
        var descriptor = _descriptors.FirstOrDefault(x => x.Name == message.Subject);

        if (descriptor is null)
        {
            throw new InvalidOperationException($"Could not find descriptor for message {message.Subject}");
        }

        return new IntegrationEvent(
            message.MessageId,
            message.Subject,
            message.ApplicationProperties["EventMinorVersion"] as int? ?? 0,
            descriptor.Parser.ParseFrom(message.Body));
    }

    public bool TryCreate(IntegrationEventServiceBusMessage message, [NotNullWhen(true)] out IntegrationEvent? integrationEvent)
    {
        var descriptor = _descriptors.FirstOrDefault(x => x.Name == message.Subject);

        integrationEvent = descriptor is not null
            ? new IntegrationEvent(
                message.MessageId,
                message.Subject,
                message.ApplicationProperties["EventMinorVersion"] as int? ?? 0,
                descriptor.Parser.ParseFrom(message.Body))
            : null;

        return integrationEvent is not null;
    }
}
