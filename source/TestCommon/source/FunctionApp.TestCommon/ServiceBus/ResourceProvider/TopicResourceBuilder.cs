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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    public class TopicResourceBuilder
    {
        internal TopicResourceBuilder(ServiceBusResourceProvider serviceBusResource, CreateTopicOptions createTopicOptions)
        {
            ServiceBusResource = serviceBusResource;
            CreateTopicOptions = createTopicOptions;

            Subscriptions = new Dictionary<string, CreateSubscriptionOptions>();
        }

        private ServiceBusResourceProvider ServiceBusResource { get; }

        private CreateTopicOptions CreateTopicOptions { get; }

        private IDictionary<string, CreateSubscriptionOptions> Subscriptions { get; }

        public TopicResourceBuilder Do(Action<TopicProperties> postAction)
        {
            // TODO: Implement "Do" actions
            return this;
        }

        /// <summary>
        /// Add a subscription to the topic we are building.
        /// </summary>
        /// <param name="subscriptionName">The subscription name.</param>
        /// <param name="maxDeliveryCount"></param>
        /// <param name="lockDuration"></param>
        /// <returns>Topic resouce builder.</returns>
        public TopicResourceBuilder AddSubscription(string subscriptionName, int maxDeliveryCount = 1, TimeSpan? lockDuration = null)
        {
            var createSubscriptionOptions = new CreateSubscriptionOptions(CreateTopicOptions.Name, subscriptionName)
            {
                AutoDeleteOnIdle = CreateTopicOptions.AutoDeleteOnIdle,
                MaxDeliveryCount = maxDeliveryCount,
                LockDuration = lockDuration ?? TimeSpan.FromMinutes(1),
            };

            Subscriptions.Add(subscriptionName, createSubscriptionOptions);

            return this;
        }

        public async Task<TopicResource> CreateAsync()
        {
            var topicResource = await CreateTopicAsync()
                .ConfigureAwait(false);

            await CreateSubscriptionsAsync()
                .ConfigureAwait(false);

            return topicResource;
        }

        private async Task<TopicResource> CreateTopicAsync()
        {
            var response = await ServiceBusResource.AdministrationClient.CreateTopicAsync(CreateTopicOptions)
                .ConfigureAwait(false);

            var topicResourceName = response.Value.Name;
            var topicResource = new TopicResource(ServiceBusResource, response.Value);
            ServiceBusResource.TopicResources.Add(topicResourceName, topicResource);

            // TODO: Call "Do" actions
            return topicResource;
        }

        private async Task CreateSubscriptionsAsync()
        {
            foreach (var subcription in Subscriptions)
            {
                // TODO: Create in parallel
                var response = await ServiceBusResource.AdministrationClient.CreateSubscriptionAsync(subcription.Value)
                    .ConfigureAwait(false);
            }
        }
    }
}
