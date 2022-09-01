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
    /// <summary>
    /// Part of fluent API for creating a Service Bus topic resource with subscriptions.
    /// </summary>
    public class TopicSubscriptionBuilder : ITopicResourceBuilder
    {
        internal TopicSubscriptionBuilder(
            TopicResourceBuilder topicResourceBuilder,
            CreateSubscriptionOptions createSubscriptionOptions,
            CreateRuleOptions? createRuleOptions)
        {
            TopicResourceBuilder = topicResourceBuilder;
            CreateRuleOptions = createRuleOptions;
            CreateSubscriptionOptions = createSubscriptionOptions;

            PostActions = new List<Action<SubscriptionProperties>>();
        }

        internal CreateSubscriptionOptions CreateSubscriptionOptions { get; }

        internal CreateRuleOptions? CreateRuleOptions { get; }

        internal IList<Action<SubscriptionProperties>> PostActions { get; }

        private TopicResourceBuilder TopicResourceBuilder { get; }

        /// <summary>
        /// Add an action that will be called after the subscription has been created.
        /// </summary>
        /// <param name="postAction">Action to call with subscription properties when subscription has been created.</param>
        /// <returns>Subscription builder.</returns>
        public TopicSubscriptionBuilder Do(Action<SubscriptionProperties> postAction)
        {
            PostActions.Add(postAction);

            return this;
        }

        /// <inheritdoc/>
        public TopicSubscriptionBuilder AddSubscription(
            string subscriptionName,
            CreateRuleOptions? createRuleOptions = null,
            int maxDeliveryCount = 1,
            TimeSpan? lockDuration = null,
            bool requiresSession = false)
        {
            return TopicResourceBuilder.AddSubscription(
                subscriptionName, createRuleOptions, maxDeliveryCount, lockDuration, requiresSession);
        }

        /// <inheritdoc/>
        public Task<TopicResource> CreateAsync()
        {
            return TopicResourceBuilder.CreateAsync();
        }
    }
}
