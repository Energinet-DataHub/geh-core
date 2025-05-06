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

using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;

/// <summary>
/// Extension methods for <see cref="IFunctionsWorkerApplicationBuilder"/>
/// that allow adding custom middleware to a Function App.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Register middleware necessary for refresing Azure App Configuration
    /// if trigger is not an Durable Function Orchestration trigger.
    /// </summary>
    /// <remarks>
    /// Inspired by the following:
    ///  - https://github.com/Azure/AppConfiguration-DotnetProvider/blob/e09cb23855a36843f8381b7eb172139a6553f0f1/src/Microsoft.Azure.AppConfiguration.Functions.Worker/AzureAppConfigurationRefreshExtensions.cs#L18
    ///  - https://github.com/Azure/azure-functions-durable-extension/blob/b49869321ea8c307af07fc5c8231aa84a177e851/src/Worker.Extensions.DurableTask/DurableTaskFunctionsMiddleware.cs#L39
    /// </remarks>
    public static IFunctionsWorkerApplicationBuilder UseAzureAppConfigurationForIsolatedWorker(
        this IFunctionsWorkerApplicationBuilder builder)
    {
        // Verify if AddAzureAppConfiguration was done before calling UseAzureAppConfiguration.
        // We use the IConfigurationRefresherProvider to make sure if the required services were added.
        IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
        var refresherProvider = serviceProvider.GetService<IConfigurationRefresherProvider>()
            ?? throw new InvalidOperationException($"Unable to find the required services. Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(AzureAppConfigurationExtensions.AddAzureAppConfiguration)}()' in the application startup code.");

        if (refresherProvider.Refreshers?.Count() > 0)
        {
            builder.UseWhen<AzureAppConfigurationRefreshMiddleware>((context) =>
            {
                var isOrchestrationTrigger = context.FunctionDefinition.InputBindings.Values
                    .First(metadata => metadata.Type.EndsWith("Trigger"))
                    .Type == "orchestrationTrigger";

                return !isOrchestrationTrigger;
            });
        }

        return builder;
    }
}
