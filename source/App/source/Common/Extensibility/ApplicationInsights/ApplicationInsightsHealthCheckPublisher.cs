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

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.App.Common.Extensibility.ApplicationInsights;

public class ApplicationInsightsHealthCheckPublisher : IHealthCheckPublisher
{
    private readonly TelemetryConfiguration _telemetryConfiguration;

    public ApplicationInsightsHealthCheckPublisher(IOptions<TelemetryConfiguration> telemetryConfiguration)
    {
        _telemetryConfiguration = telemetryConfiguration.Value;
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        var client = new TelemetryClient(_telemetryConfiguration);

        foreach (var reportEntry in report.Entries)
        {
            client.TrackEvent(
                $"AspNetCoreHealthCheck",
                properties: new Dictionary<string, string?>()
                {
                        { "Name", reportEntry.Key },
                        { "ExceptionType", reportEntry.Value.Exception?.GetType().Name },
                        { "ExceptionMessage", reportEntry.Value.Exception?.Message },
                        { "ExceptionStackTrace", reportEntry.Value.Exception?.StackTrace },
                },
                metrics: new Dictionary<string, double>()
                {
                        { $"AspNetCoreHealthCheckStatus:", (int)reportEntry.Value.Status },
                        { "AspNetCoreHealthCheckDuration", reportEntry.Value.Duration.TotalMilliseconds },
                });
        }

        await client.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
