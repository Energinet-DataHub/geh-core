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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.App.WebApp.Hosting;

/// <summary>
/// Hosted service worker repeating a task indefinitely.
/// </summary>
public abstract class RepeatingTrigger<TService> : BackgroundService, IRepeatingTrigger
    where TService : notnull
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly TimeSpan _delayBetweenExecutions;

    private DateTimeOffset _activityTimeStamp;

    protected RepeatingTrigger(
        IServiceProvider serviceProvider,
        ILogger logger,
        TimeSpan delayBetweenExecutions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _delayBetweenExecutions = delayBetweenExecutions;
    }

    public DateTimeOffset LastTimeActivityRegistered => _activityTimeStamp;

    protected ILogger Logger => _logger;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("Worker has stopped at {Time}.", DateTimeOffset.Now);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        UpdateIsActive();

        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await InvokeAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(_delayBetweenExecutions, stoppingToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Method to be implemented by the inheriting class.
    /// The method is invoked repeatedly with a delay between each invocation.
    /// Execution time should either be less than the health timeout (desirable) or alternatively
    /// the implementor must make sure to invoke the action isAliveCallback at sufficient
    /// intervals. Otherwise the service will respond as not being healthy.
    /// </summary>
    protected abstract Task ExecuteAsync(
        TService scopedService,
        CancellationToken cancellationToken,
        Action isAliveCallback);

    private async Task InvokeAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();

        try
        {
            await ExecuteAsync(service, cancellationToken, UpdateIsActive).ConfigureAwait(false);
            UpdateIsActive();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception in worker.");
        }
    }

    private void UpdateIsActive()
    {
        _activityTimeStamp = DateTimeOffset.UtcNow;
    }
}
