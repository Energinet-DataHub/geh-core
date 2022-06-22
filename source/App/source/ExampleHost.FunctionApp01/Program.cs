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
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using ExampleHost.FunctionApp01.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseMiddleware<CorrelationIdMiddleware>();
        builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
    })
    .ConfigureServices(services =>
    {
        // CONCLUSION:
        //  * We can see Trace and Request entries in App Insights out-of-box.
        //  * Dependency tracing is not support (out-of-box) in isolated-process [https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring#dependencies]

        // TODO: Investigate if any of this is relevant for us:
        //  - https://github.com/Azure/azure-functions-dotnet-worker/issues/760
        //  - https://github.com/Azure/azure-functions-dotnet-worker/issues/822#issuecomment-1088012705

        //// UNDONE: Track custom operations with App Insights SDK's [https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking]

        // CONCLUSION: We can use ILogger<> without calling the following:
        ////services.AddLogging();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<CorrelationIdMiddleware>();
        services.AddScoped<FunctionTelemetryScopeMiddleware>();

        services.AddSingleton<ServiceBusClient>(_ =>
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
    })
    .Build();

host.Run();
