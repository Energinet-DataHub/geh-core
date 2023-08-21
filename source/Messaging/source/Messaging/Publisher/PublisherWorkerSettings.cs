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

namespace Energinet.DataHub.Core.Messaging.Communication.Publisher;

/// <summary>
/// Settings for the communication with the Service Bus.
/// </summary>
public sealed class PublisherWorkerSettings
{
    /// <summary>
    /// The connection string for the Service Bus.
    /// </summary>
    public string ServiceBusIntegrationEventWriteConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The name of the topic to send integration events to.
    /// </summary>
    public string IntegrationEventTopicName { get; set; } = string.Empty;

    /// <summary>
    /// Delay in milliseconds between each execution of the hosted service.
    /// </summary>
    public int HostedServiceExecutionDelayMs { get; set; } = 10000;
}
