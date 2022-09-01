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

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    /// <summary>
    /// Part of fluent API for creating a Service Bus topic resource with subscriptions.
    /// </summary>
    public interface ITopicResourceBuilder
    {
        /// <summary>
        /// Add a subscription to the topic we are building.
        /// </summary>
        /// <param name="subscriptionName">The subscription name.</param>
        /// <param name="maxDeliveryCount"></param>
        /// <param name="lockDuration"></param>
        /// <param name="requiresSession"></param>
        /// <returns>Subscription resource builder.</returns>
        TopicSubscriptionBuilder AddSubscription(
            string subscriptionName,
            int maxDeliveryCount = 1,
            TimeSpan? lockDuration = null,
            bool requiresSession = false);

        /// <summary>
        /// Create Service Bus topic and subscription according to configured builder.
        /// </summary>
        /// <returns>Instance with information about the created topic.</returns>
        Task<TopicResource> CreateAsync();
    }
}
