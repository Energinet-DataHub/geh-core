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

using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        // Configuration verified in tests. See comments in FunctionApp01.Program.
        services.AddApplicationInsightsForIsolatedWorker(subsystemName: "ExampleHost.FunctionApp");

        // Http => Subsystem Authentication using DarkLoop Authorization extension (verified in tests)
        services
            .AddSubsystemAuthenticationForIsolatedWorker(context.Configuration);
    })
    .ConfigureFunctionsWebApplication(builder =>
    {
        // DarkLoop Authorization extension (verified in tests):
        //  * Explicitly adding the extension middleware because registering middleware when extension is loaded does not
        //    place the middleware in the pipeline where required request information is available.
        builder.UseFunctionsAuthorization();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        // Configuration verified in tests. See comments in FunctionApp01.Program.
        logging.AddLoggingConfigurationForIsolatedWorker(hostingContext.Configuration);
    })
    .Build();

host.Run();
