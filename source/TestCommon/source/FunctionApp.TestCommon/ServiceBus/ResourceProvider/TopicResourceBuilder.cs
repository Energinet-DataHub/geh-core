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

using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;

/// <summary>
/// Part of fluent API for creating a Service Bus topic resource with subscriptions.
/// </summary>
public class TopicResourceBuilder : ITopicResourceBuilder
{
    internal TopicResourceBuilder(ServiceBusResourceProvider resourceProvider, CreateTopicOptions createTopicOptions)
    {
        ResourceProvider = resourceProvider;
        CreateTopicOptions = createTopicOptions;

        PostActions = new List<Action<TopicProperties>>();
        SubscriptionBuilders = new Dictionary<string, TopicSubscriptionBuilder>();
    }

    private ServiceBusResourceProvider ResourceProvider { get; }

    private CreateTopicOptions CreateTopicOptions { get; }

    private IDictionary<string, TopicSubscriptionBuilder> SubscriptionBuilders { get; }

    private IList<Action<TopicProperties>> PostActions { get; }

    /// <summary>
    /// Add an action that will be called after the topic has been created.
    /// </summary>
    /// <param name="postAction">Action to call with topic properties when topic has been created.</param>
    /// <returns>Topic resource builder.</returns>
    public TopicResourceBuilder Do(Action<TopicProperties> postAction)
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
        var createSubscriptionOptions = new CreateSubscriptionOptions(CreateTopicOptions.Name, subscriptionName)
        {
            AutoDeleteOnIdle = CreateTopicOptions.AutoDeleteOnIdle,
            MaxDeliveryCount = maxDeliveryCount,
            LockDuration = lockDuration ?? TimeSpan.FromMinutes(1),
            RequiresSession = requiresSession,
        };

        var subscriptionBuilder = new TopicSubscriptionBuilder(this, createSubscriptionOptions);
        SubscriptionBuilders.Add(subscriptionName, subscriptionBuilder);

        return subscriptionBuilder;
    }

    /// <inheritdoc/>
    public async Task<TopicResource> CreateAsync()
    {
        var topicResource = await CreateTopicAsync()
            .ConfigureAwait(false);

        await CreateSubscriptionsAsync(topicResource)
            .ConfigureAwait(false);

        return topicResource;
    }

    private async Task<TopicResource> CreateTopicAsync()
    {
        ResourceProvider.TestLogger.WriteLine($"Creating topic '{CreateTopicOptions.Name}'");

        var response = await ResourceProvider.AdministrationClient.CreateTopicAsync(CreateTopicOptions)
            .ConfigureAwait(false);

        var topicResourceName = response.Value.Name;
        var topicResource = new TopicResource(ResourceProvider, response.Value);
        ResourceProvider.TopicResources.Add(topicResourceName, topicResource);

        foreach (var postAction in PostActions)
        {
            postAction(response.Value);
        }

        return topicResource;
    }

    private async Task CreateSubscriptionsAsync(TopicResource topicResource)
    {
        foreach (var subscriptionBuilderPair in SubscriptionBuilders)
        {
            var subscription = subscriptionBuilderPair.Value;
            ResourceProvider.TestLogger.WriteLine($"Creating subscription '{subscription.CreateSubscriptionOptions.SubscriptionName}'");
            var response = await ResourceProvider.AdministrationClient
                .CreateSubscriptionAsync(
                        subscription.CreateSubscriptionOptions,
                        subscription.CreateRuleOptions)
                .ConfigureAwait(false);

            topicResource.AddSubscription(response.Value);

            foreach (var postAction in subscriptionBuilderPair.Value.PostActions)
            {
                postAction(response.Value);
            }
        }
    }
}
