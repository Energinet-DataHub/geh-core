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

using HealthChecks.AzureServiceBus.Configuration;

namespace Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;

internal sealed class ServiceBusTopicSubscriptionDeadLetterHealthCheckOptions(string topicName, string subscriptionName)
    : AzureServiceBusHealthCheckOptions
{
    /// <summary>
    /// The name of the topic to check.
    /// </summary>
    internal string TopicName { get; } = topicName;

    /// <summary>
    /// The subscription name of the topic subscription to check.
    /// </summary>
    internal string SubscriptionName { get; } = subscriptionName;
}