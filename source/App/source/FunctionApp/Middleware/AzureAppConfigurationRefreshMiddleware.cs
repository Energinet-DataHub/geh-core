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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware;

/// <summary>
/// Custom middleware implementation for refreshing Azure App Configuration.
/// Should not be used if trigger is an Durable Function Orchestration trigger.
/// </summary>
/// <remarks>
/// As suggested here: https://github.com/Azure/azure-functions-dotnet-worker/issues/1666#issuecomment-1839370553
/// We only register this middleware to run if it's not a Durable Function Trigger (see "UseAzureAppConfigurationForIsolatedWorker").
/// The middleware implementation is a copy of the one Microsoft ahs implamanted here: https://github.com/Azure/AppConfiguration-DotnetProvider/blob/e09cb23855a36843f8381b7eb172139a6553f0f1/src/Microsoft.Azure.AppConfiguration.Functions.Worker/AzureAppConfigurationRefreshMiddleware.cs#L17
/// </remarks>
public class AzureAppConfigurationRefreshMiddleware : IFunctionsWorkerMiddleware
{
    // The minimum refresh interval on the configuration provider is 1 second, so refreshing more often is unnecessary
    private static readonly long MinimumRefreshInterval = TimeSpan.FromSeconds(1).Ticks;
    private long _refreshReadyTime = DateTimeOffset.UtcNow.Ticks;

    private IEnumerable<IConfigurationRefresher> Refreshers { get; }

    // DO NOT inject scoped services in the middleware constructor.
    // DO use scoped services in middleware by retrieving them from 'FunctionContext.InstanceServices'
    // DO NOT store scoped services in fields or properties of the middleware object. See https://github.com/Azure/azure-functions-dotnet-worker/issues/1327#issuecomment-1434408603
    public AzureAppConfigurationRefreshMiddleware(IConfigurationRefresherProvider refresherProvider)
    {
        Refreshers = refresherProvider.Refreshers;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        long utcNow = DateTimeOffset.UtcNow.Ticks;

        long refreshReadyTime = Interlocked.Read(ref _refreshReadyTime);

        if (refreshReadyTime <= utcNow &&
            Interlocked.CompareExchange(ref _refreshReadyTime, utcNow + MinimumRefreshInterval, refreshReadyTime) == refreshReadyTime)
        {
            // Configuration refresh is meant to execute as an isolated background task.
            // To prevent access of request-based resources, such as HttpContext, we suppress the execution context within the refresh operation.
            using (AsyncFlowControl flowControl = ExecutionContext.SuppressFlow())
            {
                foreach (IConfigurationRefresher refresher in Refreshers)
                {
                    _ = Task.Run(() => refresher.TryRefreshAsync());
                }
            }
        }

        await next(context).ConfigureAwait(false);
    }

}
