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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using ExampleHost.FunctionApp01.Common;
using ExampleHost.FunctionApp01.Functions;
using ExampleHost.FunctionApp01.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        // Configuration verified in tests:
        //  * Endpoints for which UserMiddleware is enabled must call the endpoint with a token
        //  * We exclude endpoints for which we in tests do not want to, or cannot, send a token
        worker.UseUserMiddlewareForIsolatedWorker<ExampleSubsystemUser>(
            excludedFunctionNames:
                [$"{nameof(MockedTokenFunction.GetToken)}",
                $"{nameof(RestApiExampleFunction.TelemetryAsync)}"]);
    })
    .ConfigureServices(services =>
    {
        // Configuration verified in tests:
        //  * Logging using ILogger<T> will work, but notice that by default we need to log as "Warning" for it to
        //    appear in Application Insights (can be configured).
        //  * We can see Trace, Request, Dependencies and other entries in App Insights out-of-box.
        //  * Telemetry events are enriched with property "Subsystem" and configured value
        services.AddApplicationInsightsForIsolatedWorker(subsystemName: "ExampleHost.FunctionApp");

        // Configure ServiceBusSender for calling FunctionApp02
        services.AddSingleton(_ =>
        {
            var connectionString = Environment.GetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventConnectionString);
            return new ServiceBusClient(connectionString);
        });
        services.AddSingleton<ServiceBusSender>(sp =>
        {
            var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
            var topicName = Environment.GetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventTopicName);
            return serviceBusClient.CreateSender(topicName);
        });

        // Health Checks (verified in tests)
        services.AddHealthChecksForIsolatedWorker();

        // Http => Authentication (verified in tests)
        services.AddUserAuthenticationForIsolatedWorker<ExampleSubsystemUser, ExampleSubsystemUserProvider>();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        // Configuration verified in tests:
        //  * Ensure Application Insights logging configuration is picked up.
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext);
    })
    .Build();

host.Run();
