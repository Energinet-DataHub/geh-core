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
using Energinet.DataHub.Core.Messaging.Communication.Configuration;
using Energinet.DataHub.Core.Messaging.Communication.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.Messaging.Communication.Extensions.Builder;

public static class ServiceBusHealthCheckBuilderExtensions
{
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
                        new ServiceBusDeadLetterHealthCheckOptions(
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
                        new ServiceBusDeadLetterHealthCheckOptions(
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
}
