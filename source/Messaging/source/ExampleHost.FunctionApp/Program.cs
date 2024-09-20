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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using ExampleHost.FunctionApp.IntegrationEvents;
using ExampleHost.FunctionApp.IntegrationEvents.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        // Application Insights and logging:
        //  * IMPORTANT: In a real DH3 applications we should use the App package and call 'AddApplicationInsightsForIsolatedWorker'
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configuration verified in tests:
        //  * The 'ISubscriber' is used in the 'IntegrationEventListener'
        //  * Register an example implementation of 'IIntegrationEventHandler' which will process the registered events
        //  * Register the event 'AcceptedV1' for processing
        //  * Notice we DO NOT register the event 'UnknownV1' so it should not be given to the handler for processing
        services.AddSubscriber<ExampleIntegrationEventHandler>(new[]
        {
            AcceptedV1.Descriptor,
        });

        // Configuration verified in tests:
        //  * The dead-letter handler is used in the 'IntegrationEventDeadLetterListener'
        services.AddDeadLetterHandlerForIsolatedWorker(context.Configuration);
    })
    .Build();

host.Run();
