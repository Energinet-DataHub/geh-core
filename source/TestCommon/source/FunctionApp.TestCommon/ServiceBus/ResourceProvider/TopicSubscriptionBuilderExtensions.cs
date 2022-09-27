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
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    public static class TopicSubscriptionBuilderExtensions
    {
        public const string DefaultSubjectRuleName = "subject-rule";

        public const string DefaultSubjectAndToRuleName = "subject-to-rule";

        public const string MessageTypeRuleName = "message-type-rule";

        public static TopicSubscriptionBuilder SetEnvironmentVariableToSubscriptionName(this TopicSubscriptionBuilder builder, string variable)
        {
            builder.Do(subscriptionProperties => Environment.SetEnvironmentVariable(variable, subscriptionProperties.SubscriptionName));

            return builder;
        }

        /// <summary>
        /// Add a subscription rule. Can be used to create a Correlation or SQL filter.
        /// </summary>
        public static TopicSubscriptionBuilder AddRule(this TopicSubscriptionBuilder builder, CreateRuleOptions ruleOptions)
        {
            if (ruleOptions is null)
            {
                throw new ArgumentNullException(nameof(ruleOptions));
            }

            builder.CreateRuleOptions = ruleOptions;

            return builder;
        }

        /// <summary>
        /// Add a correlation filter with <see cref="DefaultSubjectRuleName"/> that will filter on Subject.
        /// </summary>
        public static TopicSubscriptionBuilder AddSubjectFilter(this TopicSubscriptionBuilder builder, string subject)
        {
            builder.CreateRuleOptions = new CreateRuleOptions(DefaultSubjectRuleName, new CorrelationRuleFilter { Subject = subject });

            return builder;
        }

        /// <summary>
        /// Add a correlation filter with <see cref="DefaultSubjectAndToRuleName"/> that will filter on Subject AND To.
        /// </summary>
        public static TopicSubscriptionBuilder AddSubjectAndToFilter(this TopicSubscriptionBuilder builder, string subject, string to)
        {
            builder.CreateRuleOptions = new CreateRuleOptions(DefaultSubjectAndToRuleName, new CorrelationRuleFilter { Subject = subject, To = to });

            return builder;
        }

        /// <summary>
        /// Adds a filter on the message type of the integration events according to ADR-008.
        /// </summary>
        public static TopicSubscriptionBuilder AddMessageTypeFilter(this TopicSubscriptionBuilder builder, string messageType)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            builder.CreateRuleOptions = new CreateRuleOptions(
                MessageTypeRuleName,
                new CorrelationRuleFilter
                {
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>("MessageType", messageType),
                    },
                });

            return builder;
        }
    }
}
