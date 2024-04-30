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
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;

/// <summary>
/// Extension methods for <see cref="IFunctionsWorkerApplicationBuilder"/>
/// that allow adding authentication related middleware to a Function App.
/// </summary>
public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Register middleware necessary for enabling user authentication in a http triggered function.
    /// Ignores health check endpoints.
    /// Exclude anonymous endpoints by adding their names to the <paramref name="excludedFunctionNames"/>.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseUserMiddlewareForIsolatedWorker<TUser>(
        this IFunctionsWorkerApplicationBuilder builder,
        IReadOnlyCollection<string>? excludedFunctionNames = null)
        where TUser : class
    {
        builder.UseWhen<UserMiddleware<TUser>>((context) =>
        {
            // Only relevant for http triggers
            var isHttpTrigger = context.FunctionDefinition.InputBindings.Values
                .First(metadata => metadata.Type.EndsWith("Trigger"))
                .Type == "httpTrigger";

            // Not relevant for health check endpoint (they allow anonymous access)
            var isHealthCheckEndpoint = context.FunctionDefinition.Name == "HealthCheck";

            // This is how we should configure user authentication in most production applications
            if (!isHttpTrigger || isHealthCheckEndpoint)
            {
                return false;
            }

            // Support excluding anonymous endpoints
            return excludedFunctionNames == null
                || !excludedFunctionNames.Contains(context.FunctionDefinition.Name);
        });

        return builder;
    }
}
