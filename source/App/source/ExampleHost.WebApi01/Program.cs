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

using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using ExampleHost.WebApi01;

var builder = WebApplication.CreateBuilder(args);

// Must be configured in ExampleHostFixture as well.
builder.Configuration.AddAzureAppConfigurationForWebApp(builder.Configuration);

// We keep the Startup to be able to create Web01Host using TestServer in integration tests.
var startup = new Startup(builder.Configuration);

/*
// Add services to the container.
*/
startup.ConfigureServices(builder.Services);

var app = builder.Build();

/*
// Configure the HTTP request pipeline.
*/
startup.Configure(app, app.Environment);

app.Run();
