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
using Energinet.DataHub.Core.App.Common.Extensibility.ApplicationInsights;
using Energinet.DataHub.Core.App.Common.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding Application Insights services to a Function App.
/// </summary>
public static class ApplicationInsightsExtensions
{
    /// <summary>
    /// Register services necessary for enabling an Azure Function App (isolated worker model)
    /// to log telemetry to Application Insights.
    ///
    /// Configuration of telemetry (initializers, properties etc.) within the isolated worker
    /// only affects logs emitted from the isolated worker and not those emitted from the host.
    /// Logs emitted from the isolated worker will have the following properties set:
    ///  - "AppVersion" is set according to the AssemblyInformationalVersion of the isolated worker.
    ///  - "Subsystem" is set to value given by <paramref name="subsystemName"/>.
    /// </summary>
    public static IServiceCollection AddApplicationInsightsForIsolatedWorker(this IServiceCollection services, string subsystemName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subsystemName);

        // Telemetry initializers only adds information to logs emitted by the isolated worker; not logs emitted by the function host.
        services.TryAddSingleton<ITelemetryInitializer>(new SubsystemInitializer(subsystemName));

        // Configure isolated worker to emit logs directly to Application Insights.
        // See https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
        services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.ApplicationVersion = Assembly
                .GetEntryAssembly()!
                .GetAssemblyInformationalVersionAttribute()!
                .GetSourceVersionInformation()
                .ToString();
        });
        services.ConfigureFunctionsApplicationInsights();

        return services;
    }
}
