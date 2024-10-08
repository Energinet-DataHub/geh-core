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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Publisher;

[Obsolete("This implementation isn't thread safe, and we should use Azure SDK extensions for client registrations and lifetime handling.")]
internal sealed class ServiceBusSenderProvider : IServiceBusSenderProvider
{
    private readonly IOptions<PublisherOptions> _options;
    private ServiceBusSender? _serviceBusSender;

    public ServiceBusSenderProvider(IOptions<PublisherOptions> options)
    {
        _options = options;
    }

    public ServiceBusSender Instance
        => _serviceBusSender ??= CreateServiceBusClient()
            .CreateSender(_options.Value.TopicName);

    private ServiceBusClient CreateServiceBusClient()
    {
        return new ServiceBusClient(
            _options.Value.ServiceBusConnectionString,
            new ServiceBusClientOptions
            {
                TransportType = _options.Value.TransportType,
            });
    }
}
