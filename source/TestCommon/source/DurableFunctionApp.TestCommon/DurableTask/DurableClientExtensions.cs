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

using Energinet.DataHub.Core.TestCommon;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Energinet.DataHub.Core.DurableFunctionApp.TestCommon.DurableTask;

public static class DurableClientExtensions
{
    private const int WaitTimeLimit = 60;
    private const int DelayLimit = 5;

    /// <summary>
    /// Wait for an orchestration that is either running or completed,
    /// and which was started at, or later, than given <paramref name="createdTimeFrom"/>.
    ///
    /// If more than one orchestration exists an exception is thrown.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="createdTimeFrom">The orchestration must be started after this datetime (in UTC).</param>
    /// <param name="name">If provided, the orchestration name must be equal to this value (case insensitive).</param>
    /// <param name="waitTimeLimit">Max time to wait for orchestration. If not specified it defaults to the value of<see cref="WaitTimeLimit"/> in seconds.</param>
    /// <returns>If started within given <paramref name="waitTimeLimit"/> it returns the orchestration status; otherwise it throws an exception.</returns>
    public static async Task<DurableOrchestrationStatus> WaitForOrchestationStartedAsync(
        this IDurableClient client,
        DateTime createdTimeFrom,
        string? name = null,
        TimeSpan? waitTimeLimit = null)
    {
        var filter = new OrchestrationStatusQueryCondition()
        {
            CreatedTimeFrom = createdTimeFrom,
            RuntimeStatus =
            [
                OrchestrationRuntimeStatus.Pending,
                OrchestrationRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Completed,
                OrchestrationRuntimeStatus.Failed,
            ],
        };

        IReadOnlyCollection<DurableOrchestrationStatus> durableOrchestrationState = [];
        var isAvailable = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                var queryResult = await client.ListInstancesAsync(filter, CancellationToken.None).ConfigureAwait(false);

                if (queryResult == null)
                    return false;

                durableOrchestrationState = queryResult.DurableOrchestrationState
                    .Where(o => name == null || o.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (durableOrchestrationState.Count == 0)
                    return false;

                if (durableOrchestrationState.Count > 1)
                    throw new Exception($"Unexpected amount of orchestration instances found. Expected 1, but found {durableOrchestrationState.Count}");

                return durableOrchestrationState.Single().RuntimeStatus == OrchestrationRuntimeStatus.Failed
                        ? throw new Exception($"Orchestration has failed.")
                        : true;
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(WaitTimeLimit),
            delay: TimeSpan.FromSeconds(DelayLimit)).ConfigureAwait(false);

        return isAvailable
            ? durableOrchestrationState.Single()
            : throw new Exception($"Orchestration did not start within configured wait time limit.");
    }

    /// <summary>
    /// Wait for an orchestration instance to reach state <see cref="OrchestrationRuntimeStatus.Completed"/> within given <paramref name="waitTimeLimit"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="instanceId">The instance id of the orchestration to wait for.</param>
    /// <param name="waitTimeLimit">Max time to wait for the state. If not specified it defaults to the value of <see cref="WaitTimeLimit"/> in seconds.</param>
    /// <returns>If state is reached within given <paramref name="waitTimeLimit"/> it returns the orchestration status including history; otherwise it throws an exception.</returns>
    public static Task<DurableOrchestrationStatus> WaitForOrchestrationCompletedAsync(
        this IDurableClient client,
        string instanceId,
        TimeSpan? waitTimeLimit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        return WaitForOrchestrationStateAsync(
            client,
            instanceId,
            waitTimeLimit,
            expectedState: OrchestrationRuntimeStatus.Completed);
    }

    /// <summary>
    /// Wait for an orchestration instance to reach state <see cref="OrchestrationRuntimeStatus.Running"/> within given <paramref name="waitTimeLimit"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="instanceId">The instance id of the orchestration to wait for.</param>
    /// <param name="waitTimeLimit">Max time to wait for the state. If not specified it defaults to the value of <see cref="WaitTimeLimit"/> in seconds.</param>
    /// <returns>If state is reached within given <paramref name="waitTimeLimit"/> it returns the orchestration status including history; otherwise it throws an exception.</returns>
    public static Task<DurableOrchestrationStatus> WaitForOrchestrationRunningAsync(
        this IDurableClient client,
        string instanceId,
        TimeSpan? waitTimeLimit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        return WaitForOrchestrationStateAsync(
            client,
            instanceId,
            waitTimeLimit,
            expectedState: OrchestrationRuntimeStatus.Running);
    }

    /// <summary>
    /// Wait for orchestration instance to have a custom status where the given <paramref name="matchFunction"/> returns true
    /// </summary>
    /// <param name="client"></param>
    /// <param name="instanceId">The instance id of the orchestration to wait for.</param>
    /// <param name="matchFunction">Predicate to determine if custom status is a match.</param>
    /// <param name="waitTimeLimit">Max time to wait for custom status. If not specified it defaults to the value of <see cref="WaitTimeLimit"/> in seconds.</param>
    /// <returns>If completed within given <paramref name="waitTimeLimit"/> it returns the orchestration status including history; otherwise it throws an exception.</returns>
    /// <exception cref="InvalidCastException">Throws InvalidCastException if CustomStatus property cannot be parsed to given type</exception>
    public static async Task<DurableOrchestrationStatus> WaitForCustomStatusAsync<TCustomStatus>(
        this IDurableClient client,
        string instanceId,
        Func<TCustomStatus, bool> matchFunction,
        TimeSpan? waitTimeLimit = null)
    {
        var matchesCustomStatus = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                // Do not retrieve history here as it could be expensive
                var orchestrationStatus = await client.GetStatusAsync(instanceId).ConfigureAwait(false);

                var customStatus = orchestrationStatus.CustomStatus.ToObject<TCustomStatus>()
                    ?? throw new InvalidCastException($"Cannot cast CustomStatus to {typeof(TCustomStatus).Name}");

                var isMatch = matchFunction(customStatus);

                return isMatch;
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(WaitTimeLimit),
            delay: TimeSpan.FromSeconds(DelayLimit)).ConfigureAwait(false);

        var actualStatus = await client.GetStatusAsync(instanceId, showHistory: true, showHistoryOutput: true).ConfigureAwait(false);
        return matchesCustomStatus
            ? actualStatus
            : throw new Exception($"Orchestration instance '{instanceId}' did not match custom status within configured wait time limit. Actual status: {actualStatus.CustomStatus.ToString(Formatting.Indented)}");
    }

    private static async Task<DurableOrchestrationStatus> WaitForOrchestrationStateAsync(
        IDurableClient client,
        string instanceId,
        TimeSpan? waitTimeLimit,
        OrchestrationRuntimeStatus expectedState)
    {
        DurableOrchestrationStatus? orchestrationStatus = null;
        var isExpectedState = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                // Do not retrieve history here as it could be expensive
                orchestrationStatus = await client.GetStatusAsync(instanceId).ConfigureAwait(false);

                if (orchestrationStatus.RuntimeStatus == expectedState)
                    return true;

                return orchestrationStatus.RuntimeStatus switch
                {
                        OrchestrationRuntimeStatus.Failed or
                        OrchestrationRuntimeStatus.Suspended or
                        OrchestrationRuntimeStatus.Canceled or
                        OrchestrationRuntimeStatus.Terminated
                            => throw new Exception($"Orchestration with instanceId `{instanceId}` has an unexpected state. Actual state=`{orchestrationStatus.RuntimeStatus}`"),
                        _ => false,
                };
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(WaitTimeLimit),
            delay: TimeSpan.FromSeconds(DelayLimit)).ConfigureAwait(false);

        return isExpectedState
            ? await client.GetStatusAsync(instanceId, showHistory: true, showHistoryOutput: true).ConfigureAwait(false)
            : throw new Exception($"Orchestration instance '{instanceId}' did not reach expected state within configured wait time limit. Actual state=`{orchestrationStatus?.RuntimeStatus}`");
    }
}
