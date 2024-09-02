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

using Energinet.DataHub.Core.Messaging.Communication.Configuration;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.Messaging.Communication;

/// <summary>
/// This health check verifies that a subscription for a given topic has no dead-letter messages.
/// If dead-letter messages are found, the health check will return a failure status.
/// The health check will return a healthy status if no dead-letter messages are found.
/// This check must only ever be used for dead-letter validation.
/// For ensuring that a given topic + subscription relationship is healthy, use the <see cref="AzureServiceBusSubscriptionHealthCheck"/>.
/// Thus, it is advisable to use both health checks in conjunction.
/// </summary>
public sealed class ServiceBusDeadLetterHealthCheck
    : AzureServiceBusHealthCheck<ServiceBusDeadLetterHealthCheckOptions>, IHealthCheck
{
    public ServiceBusDeadLetterHealthCheck(
        ServiceBusDeadLetterHealthCheckOptions options,
        ServiceBusClientProvider clientProvider)
        : base(options, clientProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.TopicName);
        ArgumentException.ThrowIfNullOrEmpty(options.SubscriptionName);
    }

    public ServiceBusDeadLetterHealthCheck(ServiceBusDeadLetterHealthCheckOptions options)
        : this(options, new ServiceBusClientProvider())
    {
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var managementClient = CreateManagementClient();

            var properties = await managementClient.GetSubscriptionRuntimePropertiesAsync(
                    Options.TopicName,
                    Options.SubscriptionName,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!properties.HasValue)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    $"No runtime properties found for subscription '{Options.SubscriptionName}'.");
            }

            if (properties.Value.DeadLetterMessageCount > 0)
            {
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    $"Subscription '{Options.SubscriptionName}' for topic '{Options.TopicName}' has dead-letter messages.");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
