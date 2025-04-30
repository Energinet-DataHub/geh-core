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
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

/*
// Services
*/

// Configuration verified in tests. See comments in FunctionApp01.Program.
builder.Services.AddApplicationInsightsForIsolatedWorker(subsystemName: "ExampleHost.FunctionApp");

/*
// Logging
*/

// Configuration verified in tests. See comments in FunctionApp01.Program.
builder.Logging.AddLoggingConfigurationForIsolatedWorker(builder.Configuration);

/*
// ASP.NET Core Integration
*/

builder.ConfigureFunctionsWebApplication();

/*
// Run
*/

builder.Build().Run();
