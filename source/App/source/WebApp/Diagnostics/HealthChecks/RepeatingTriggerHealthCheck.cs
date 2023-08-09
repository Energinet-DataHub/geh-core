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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.WebApp.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.App.WebApp.Diagnostics.HealthChecks;

public class RepeatingTriggerHealthCheck<TRepeatingTrigger> : IHealthCheck
    where TRepeatingTrigger : class, IRepeatingTrigger
{
    private readonly TRepeatingTrigger _repeatingTrigger;
    private readonly IOptions<RepeatingTriggerHealthCheckOptions<TRepeatingTrigger>> _options;

    public RepeatingTriggerHealthCheck(
        TRepeatingTrigger repeatingTrigger,
        IOptions<RepeatingTriggerHealthCheckOptions<TRepeatingTrigger>> options)
    {
        _repeatingTrigger = repeatingTrigger;
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var isHealthy = _repeatingTrigger.LastTimeActivityRegistered + _options.Value.HealthTimeout > DateTimeOffset.UtcNow;
        if (!isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"{_repeatingTrigger.GetType().Name} is not healthy. Last sign of life was {_repeatingTrigger.LastTimeActivityRegistered}."));
        }

        return Task.FromResult(HealthCheckResult.Healthy($"{_repeatingTrigger.GetType().Name} is healthy."));
    }
}
