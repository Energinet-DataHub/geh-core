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

using Azure.Core;
using Energinet.DataHub.Core.Messaging.Communication.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;

public static class ServiceBusHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check that verifies that a subscription for a given topic has no dead-letter messages.
    /// Note the following:
    /// <p>
    /// The health check verifies that a subscription for a given topic has no dead-letter messages.
    /// If dead-letter messages are found, the health check will return a failure status.
    /// The health check will return a healthy status if no dead-letter messages are found.
    /// This check must only ever be used for dead-letter validation.
    /// For ensuring that a given topic and subscription relationship is healthy,
    /// use the <see cref="AzureServiceBusSubscriptionHealthCheck"/> which can be added
    /// using <see cref="AzureServiceBusHealthCheckBuilderExtensions" />.
    /// </p>
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="connectionStringFactory">A factory to create the connection string.</param>
    /// <param name="topicNameFactory">A factory to create the topic name.</param>
    /// <param name="subscriptionNameFactory">A factory to create the subscription name.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags that can be used to filter health checks.</param>
    [Obsolete("This method is obsolete as we want to use IAM on the service bus.")]
    public static IHealthChecksBuilder AddServiceBusTopicSubscriptionDeadLetter(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        string name,
        IEnumerable<string>? tags = default)
    {
        ArgumentNullException.ThrowIfNull(connectionStringFactory);
        ArgumentNullException.ThrowIfNull(topicNameFactory);
        ArgumentNullException.ThrowIfNull(subscriptionNameFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return builder.Add(
            new HealthCheckRegistration(
                name,
                sp =>
                {
                    var options =
                        new ServiceBusTopicSubscriptionDeadLetterHealthCheckOptions(
                            topicNameFactory(sp),
                            subscriptionNameFactory(sp))
                        {
                            ConnectionString = connectionStringFactory(sp),
                        };

                    return new ServiceBusTopicSubscriptionDeadLetterHealthCheck(options);
                },
                HealthStatus.Unhealthy,
                tags,
                timeout: default));
    }

    /// <summary>
    /// Add a health check that verifies that a subscription for a given topic has no dead-letter messages.
    /// Note the following:
    /// <p>
    /// The health check verifies that a subscription for a given topic has no dead-letter messages.
    /// If dead-letter messages are found, the health check will return a failure status.
    /// The health check will return a healthy status if no dead-letter messages are found.
    /// This check must only ever be used for dead-letter validation.
    /// For ensuring that a given topic and subscription relationship is healthy,
    /// use the <see cref="AzureServiceBusSubscriptionHealthCheck"/> which can be added
    /// using <see cref="AzureServiceBusHealthCheckBuilderExtensions" />.
    /// </p>
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to create the namespace.</param>
    /// <param name="topicNameFactory">A factory to create the topic name.</param>
    /// <param name="subscriptionNameFactory">A factory to create the subscription name.</param>
    /// <param name="tokenCredentialFactory">A factory to create the token credential factory.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags that can be used to filter health checks.</param>
    public static IHealthChecksBuilder AddServiceBusTopicSubscriptionDeadLetter(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        string name,
        IEnumerable<string>? tags = default)
    {
        ArgumentNullException.ThrowIfNull(fullyQualifiedNamespaceFactory);
        ArgumentNullException.ThrowIfNull(topicNameFactory);
        ArgumentNullException.ThrowIfNull(subscriptionNameFactory);
        ArgumentNullException.ThrowIfNull(tokenCredentialFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return builder.Add(
            new HealthCheckRegistration(
                name,
                sp =>
                {
                    var options =
                        new ServiceBusTopicSubscriptionDeadLetterHealthCheckOptions(
                            topicNameFactory(sp),
                            subscriptionNameFactory(sp))
                        {
                            FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                            Credential = tokenCredentialFactory(sp),
                        };

                    return new ServiceBusTopicSubscriptionDeadLetterHealthCheck(options);
                },
                HealthStatus.Unhealthy,
                tags,
                timeout: default));
    }

    /// <summary>
    /// Add a health check that verifies that a queue has no dead-letter messages.
    /// Note the following:
    /// <p>
    /// The health check verifies that a subscription for a given topic has no dead-letter messages.
    /// If dead-letter messages are found, the health check will return a failure status.
    /// The health check will return a healthy status if no dead-letter messages are found.
    /// This check must only ever be used for dead-letter validation.
    /// For ensuring that a given topic and subscription relationship is healthy,
    /// use the <see cref="AzureServiceBusSubscriptionHealthCheck"/> which can be added
    /// using <see cref="AzureServiceBusHealthCheckBuilderExtensions" />.
    /// </p>
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="connectionStringFactory">A factory to create the connection string.</param>
    /// <param name="queueNameFactory">A factory to create the queue name.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags that can be used to filter health checks.</param>
    [Obsolete("This method is obsolete as we want to use IAM on the service bus.")]
    public static IHealthChecksBuilder AddServiceBusQueueDeadLetter(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> queueNameFactory,
        string name,
        IEnumerable<string>? tags = default)
    {
        ArgumentNullException.ThrowIfNull(connectionStringFactory);
        ArgumentNullException.ThrowIfNull(queueNameFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return builder.Add(
            new HealthCheckRegistration(
                name,
                sp =>
                {
                    var options = new ServiceBusQueueDeadLetterHealthCheckOptions(queueNameFactory(sp))
                    {
                        ConnectionString = connectionStringFactory(sp),
                    };

                    return new ServiceBusQueueDeadLetterHealthCheck(options);
                },
                HealthStatus.Unhealthy,
                tags,
                default));
    }

    /// <summary>
    /// Add a health check that verifies that a queue has no dead-letter messages.
    /// Note the following:
    /// <p>
    /// The health check verifies that a subscription for a given topic has no dead-letter messages.
    /// If dead-letter messages are found, the health check will return a failure status.
    /// The health check will return a healthy status if no dead-letter messages are found.
    /// This check must only ever be used for dead-letter validation.
    /// For ensuring that a given topic and subscription relationship is healthy,
    /// use the <see cref="AzureServiceBusSubscriptionHealthCheck"/> which can be added
    /// using <see cref="AzureServiceBusHealthCheckBuilderExtensions" />.
    /// </p>
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="fullyQualifiedNamespaceFactory">A factory to create the namespace.</param>
    /// <param name="queueNameFactory">A factory to create the queue name.</param>
    /// <param name="tokenCredentialFactory">A factory to create the token credential factory.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="tags">Tags that can be used to filter health checks.</param>
    public static IHealthChecksBuilder AddServiceBusQueueDeadLetter(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> fullyQualifiedNamespaceFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        string name,
        IEnumerable<string>? tags = default)
    {
        ArgumentNullException.ThrowIfNull(fullyQualifiedNamespaceFactory);
        ArgumentNullException.ThrowIfNull(queueNameFactory);
        ArgumentNullException.ThrowIfNull(tokenCredentialFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return builder.Add(
            new HealthCheckRegistration(
                name,
                sp =>
                {
                    var options = new ServiceBusQueueDeadLetterHealthCheckOptions(queueNameFactory(sp))
                    {
                        FullyQualifiedNamespace = fullyQualifiedNamespaceFactory(sp),
                        Credential = tokenCredentialFactory(sp),
                    };

                    return new ServiceBusQueueDeadLetterHealthCheck(options);
                },
                HealthStatus.Unhealthy,
                tags,
                default));
    }
}
