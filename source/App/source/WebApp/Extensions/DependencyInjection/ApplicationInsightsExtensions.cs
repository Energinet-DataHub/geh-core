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

using System.Reflection;
using Energinet.DataHub.Core.App.Common.Extensibility.ApplicationInsights;
using Energinet.DataHub.Core.App.Common.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding Application Insights services to an ASP.NET Core app.
/// </summary>
public static class ApplicationInsightsExtensions
{
    /// <summary>
    /// This operation is idempotent.
    ///
    /// Register services necessary for enabling an ASP.NET Core app to log telemetry
    /// to Application Insights.
    ///
    /// Health Checks are published to Application Insights using <see cref="ApplicationInsightsHealthCheckPublisher"/>.
    ///
    /// Tracked events will have the following properties set:
    ///  - "AppVersion" is set according to the AssemblyInformationalVersion of the host.
    ///  - "Subsystem" is set to value given by <paramref name="subsystemName"/>.
    /// </summary>
    public static IServiceCollection AddApplicationInsightsForWebApp(this IServiceCollection services, string subsystemName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subsystemName);

        services.TryAddSingleton<ITelemetryInitializer>(new SubsystemInitializer(subsystemName));

        // See https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcorenew%2Cnetcore6#enable-application-insights-server-side-telemetry-no-visual-studio
        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ApplicationVersion = Assembly
                .GetEntryAssembly()!
                .GetAssemblyInformationalVersionAttribute()!
                .GetSourceVersionInformation()
                .ToString();
        });

        services.TryAddSingleton<IHealthCheckPublisher, ApplicationInsightsHealthCheckPublisher>();

        return services;
    }
}
