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

using System.Reflection;
using Energinet.DataHub.Core.App.Common.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.App.Common.Extensibility.ApplicationInsights;

/// <summary>
/// Publishes health check reports to Application Insights.
/// See https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0#health-check-publisher for documentation.
/// </summary>
public class ApplicationInsightsHealthCheckPublisher : IHealthCheckPublisher
{
    private readonly TelemetryConfiguration _telemetryConfiguration;
    private readonly SourceVersionInformation _sourceVersionInformation;

    public ApplicationInsightsHealthCheckPublisher(IOptions<TelemetryConfiguration> telemetryConfiguration)
    {
        _telemetryConfiguration = telemetryConfiguration.Value;

        _sourceVersionInformation = Assembly
            .GetEntryAssembly()!
            .GetAssemblyInformationalVersionAttribute()!
            .GetSourceVersionInformation();
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        var client = new TelemetryClient(_telemetryConfiguration);

        foreach (var reportEntry in report.Entries)
        {
            client.TrackEvent(
                "AspNetCoreHealthCheck",
                properties: new Dictionary<string, string?>
                {
                    { "Name", reportEntry.Key },
                    { "ExceptionType", reportEntry.Value.Exception?.GetType().FullName },
                    { "ExceptionMessage", reportEntry.Value.Exception?.Message },
                    { "ExceptionStackTrace", reportEntry.Value.Exception?.StackTrace },
                    { "ProductVersion", _sourceVersionInformation.ProductVersion },
                    { "PullRequestNumber", _sourceVersionInformation.PullRequestNumber },
                    { "CommitSha", _sourceVersionInformation.LastMergeCommitSha },
                    { "Tags", System.Text.Json.JsonSerializer.Serialize(reportEntry.Value.Tags) },
                },
                metrics: new Dictionary<string, double>()
                {
                    { "AspNetCoreHealthCheckStatus", (int)reportEntry.Value.Status },
                    { "AspNetCoreHealthCheckDuration", reportEntry.Value.Duration.TotalMilliseconds },
                });
        }

        return Task.CompletedTask;
    }
}
