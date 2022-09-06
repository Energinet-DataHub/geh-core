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
        public const string DefaultSubjectRuleName = "subject-rule";

        public const string DefaultSubjectAndToRuleName = "subject-to-rule";

        internal TopicSubscriptionBuilder(
            TopicResourceBuilder topicResourceBuilder,
            CreateSubscriptionOptions createSubscriptionOptions)
        {
            TopicResourceBuilder = topicResourceBuilder;
            CreateSubscriptionOptions = createSubscriptionOptions;
            CreateRuleOptions = new CreateRuleOptions();

            PostActions = new List<Action<SubscriptionProperties>>();
        }

        internal CreateSubscriptionOptions CreateSubscriptionOptions { get; }

        internal CreateRuleOptions CreateRuleOptions { get; private set; }

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
            int maxDeliveryCount = 1,
            TimeSpan? lockDuration = null,
            bool requiresSession = false)
        {
            return TopicResourceBuilder.AddSubscription(
                subscriptionName, maxDeliveryCount, lockDuration, requiresSession);
        }

        /// <inheritdoc/>
        public Task<TopicResource> CreateAsync()
        {
            return TopicResourceBuilder.CreateAsync();
        }

        /// <summary>
        /// Add a correlation filter with <see cref="DefaultSubjectRuleName"/> that will filter on Subject.
        /// </summary>
        public TopicSubscriptionBuilder AddSubjectFilter(string subject)
        {
            CreateRuleOptions = new CreateRuleOptions(DefaultSubjectRuleName, new CorrelationRuleFilter { Subject = subject });

            return this;
        }

        /// <summary>
        /// Add a correlation filter with <see cref="DefaultSubjectAndToRuleName"/> that will filter on Subject AND To.
        /// </summary>
        public TopicSubscriptionBuilder AddSubjectAndToFilter(string subject, string to)
        {
            CreateRuleOptions = new CreateRuleOptions(DefaultSubjectAndToRuleName, new CorrelationRuleFilter { Subject = subject, To = to });

            return this;
        }
    }
}
