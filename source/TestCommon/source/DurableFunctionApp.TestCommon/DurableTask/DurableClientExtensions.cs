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
    /// <summary>
    /// Wait for an orchestration that is either running or completed,
    /// and which was started at, or later, than given <paramref name="createdTimeFrom"/>.
    ///
    /// If more than one orchestration exists an exception is thrown.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="createdTimeFrom"></param>
    /// <param name="name"></param>
    /// <param name="waitTimeLimit">Max time to wait for orchestration. If not specified it defaults to 30 seconds.</param>
    /// <returns>If started within given <paramref name="waitTimeLimit"/> it returns the orchestration status; otherwise it throws an exception.</returns>
    public static async Task<DurableOrchestrationStatus> WaitForOrchestrationStatusAsync(
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

        IEnumerable<DurableOrchestrationStatus> durableOrchestrationState = [];
        var isAvailable = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                var queryResult = await client.ListInstancesAsync(filter, CancellationToken.None).ConfigureAwait(false);

                if (queryResult == null)
                    return false;

                durableOrchestrationState = queryResult.DurableOrchestrationState
                    .Where(o => name == null || o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                return durableOrchestrationState.Any();
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(60),
            delay: TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        return isAvailable
            ? durableOrchestrationState.Single()
            : throw new Exception($"Orchestration did not start within configured wait time limit.");
    }

    /// <summary>
    /// Search for an orchestration that is either running or completed,
    /// and which was started at, or later, than given <paramref name="createdTimeFrom"/>.
    ///
    /// If more than one orchestration exists an exception is thrown.
    /// </summary>
    public static async Task<DurableOrchestrationStatus> WaitForOrchestationStartedAsync(
        this IDurableClient client,
        DateTime createdTimeFrom,
        TimeSpan? waitTimeLimit = null)
    {
        IReadOnlyCollection<DurableOrchestrationStatus> orchestrationInstances = [];
        var orchestrationFound = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                var orchestrationsQueryResult = await GetOrchestrationInstancesAsync(client, createdTimeFrom).ConfigureAwait(false);
                orchestrationInstances = orchestrationsQueryResult.DurableOrchestrationState.ToList();

                return orchestrationInstances.Count > 0;
            },
            timeLimit: waitTimeLimit ?? TimeSpan.FromSeconds(60),
            delay: TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        if (!orchestrationFound)
            throw new Exception($"No orchestration instance found within configured wait time limit.");

        if (orchestrationInstances.Count > 1)
            throw new Exception($"More than one orchestration instance found. Expected 1, but found {orchestrationInstances.Count}");

        return orchestrationInstances.Single();
    }

    /// <summary>
    /// Wait for orchestration instance to be completed within given <paramref name="waitTimeLimit"/>.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="instanceId"></param>
    /// <param name="waitTimeLimit">Max time to wait for completion. If not specified it defaults to 30 seconds.</param>
    /// <returns>If completed within given <paramref name="waitTimeLimit"/> it returns the orchestration status including history; otherwise it throws an exception.</returns>
    public static async Task<DurableOrchestrationStatus> WaitForInstanceCompletedAsync(
        this IDurableClient client,
        string instanceId,
        TimeSpan? waitTimeLimit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        var isCompleted = await Awaiter.TryWaitUntilConditionAsync(
            async () =>
            {
                // Do not retrieve history here as it could be expensive
                var completeOrchestrationStatus = await client.GetStatusAsync(instanceId).ConfigureAwait(false);

                if (completeOrchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
                    throw new Exception($"Orchestration instance '{instanceId}' was status 'failed'.");

                return completeOrchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed;
            },
            waitTimeLimit ?? TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        return isCompleted
            ? await client.GetStatusAsync(instanceId, showHistory: true, showHistoryOutput: true).ConfigureAwait(false)
            : throw new Exception($"Orchestration instance '{instanceId}' did not complete within configured wait time limit.");
    }

    /// <summary>
    /// Wait for orchestration instance to have a custom status where the given <paramref name="matchFunction"/> returns true
    /// </summary>
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
            waitTimeLimit ?? TimeSpan.FromSeconds(30),
            delay: TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        var actualStatus = await client.GetStatusAsync(instanceId, showHistory: true, showHistoryOutput: true).ConfigureAwait(false);
        return matchesCustomStatus
            ? actualStatus
            : throw new Exception($"Orchestration instance '{instanceId}' did not match custom status within configured wait time limit. Actual status: {actualStatus.CustomStatus.ToString(Formatting.Indented)}");
    }

    private static async Task<OrchestrationStatusQueryResult> GetOrchestrationInstancesAsync(IDurableClient client, DateTime createdTimeFrom)
    {
        var filter = new OrchestrationStatusQueryCondition()
        {
            CreatedTimeFrom = createdTimeFrom,
            RuntimeStatus =
            [
                OrchestrationRuntimeStatus.Running,
                OrchestrationRuntimeStatus.Completed,
            ],
        };
        var queryResult = await client.ListInstancesAsync(filter, CancellationToken.None).ConfigureAwait(false);
        return queryResult;
    }
}