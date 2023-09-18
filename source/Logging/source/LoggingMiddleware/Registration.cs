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

using Energinet.DataHub.Core.Logging.LoggingMiddleware.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Core.Logging.LoggingMiddleware;

public static class Registration
{
    public static IServiceCollection AddHttpLoggingScope(this IServiceCollection services, string domain)
    {
        RegisterLoggingScope(services, domain);
        services.AddScoped<HttpLoggingScopeMiddleware>();

        return services;
    }

    public static IServiceCollection AddFunctionLoggingScope(this IServiceCollection services, string domain)
    {
        RegisterLoggingScope(services, domain);
        services.AddScoped<FunctionLoggingScopeMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseLoggingScope(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HttpLoggingScopeMiddleware>();
    }

    public static IFunctionsWorkerApplicationBuilder UseLoggingScope(this IFunctionsWorkerApplicationBuilder builder)
    {
        return builder.UseMiddleware<FunctionLoggingScopeMiddleware>();
    }

    private static void RegisterLoggingScope(IServiceCollection services, string domain)
    {
        services.AddScoped<RootLoggingScope>(_ => new RootLoggingScope(domain));
        services.AddScoped<LoggingScope>();
    }
}
