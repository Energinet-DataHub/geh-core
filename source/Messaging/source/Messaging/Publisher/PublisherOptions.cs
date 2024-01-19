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

namespace Energinet.DataHub.Core.Messaging.Communication.Publisher;

/// <summary>
/// Settings for the communication with the Service Bus.
/// </summary>
public sealed class PublisherOptions
{
    /// <summary>
    /// The connection string for the Service Bus.
    /// </summary>
    public string ServiceBusConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The name of the topic to send integration events to.
    /// </summary>
    public string TopicName { get; set; } = string.Empty;

    /// <summary>
    /// The type of protocol and transport that will be used for communicating with the Service Bus.
    /// Default value is <see cref="ServiceBusTransportType.AmqpTcp"/>.
    /// </summary>
    public ServiceBusTransportType TransportType { get; set; } = ServiceBusTransportType.AmqpTcp;
}
