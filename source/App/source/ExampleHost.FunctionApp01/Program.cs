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

using ExampleHost.FunctionApp01.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// TODO: Investigate if any of this is relevant for us:
//  - https://github.com/Azure/azure-functions-dotnet-worker/issues/760
//  - https://github.com/Azure/azure-functions-dotnet-worker/issues/822#issuecomment-1088012705
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // CONCLUSION: We can see Trace and Request entries in App Insights even without calling this:
        ////services.AddApplicationInsightsTelemetryWorkerService(
        ////    Environment.GetEnvironmentVariable(EnvironmentSettingNames.AppInsightsInstrumentationKey));
    })
    .Build();

host.Run();
