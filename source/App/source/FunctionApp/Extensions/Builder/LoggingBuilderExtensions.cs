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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;

/// <summary>
/// Extension methods for setting up logging services in an <see cref="ILoggingBuilder" />.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// For use in a Function App isolated worker.
    /// Make sure the Application Insights logging configuration is picked up from settings.
    /// Found inspiration in https://github.com/Azure/azure-functions-dotnet-worker/issues/1447
    /// </summary>
    [Obsolete("Use overload that takes 'IConfiguration' instead. This can also be used with 'IHostApplicationBuilder'.")]
    public static ILoggingBuilder AddLoggingConfigurationForIsolatedWorker(this ILoggingBuilder logging, HostBuilderContext hostingContext)
    {
        ArgumentNullException.ThrowIfNull(hostingContext);

        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext.Configuration);

        return logging;
    }

    /// <summary>
    /// For use in a Function App isolated worker.
    /// Make sure the Application Insights logging configuration is picked up from settings.
    /// Found inspiration in https://github.com/Azure/azure-functions-dotnet-worker/issues/1447
    /// </summary>
    public static ILoggingBuilder AddLoggingConfigurationForIsolatedWorker(this ILoggingBuilder logging, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        logging.AddConfiguration(configuration.GetSection("Logging"));

        return logging;
    }
}
