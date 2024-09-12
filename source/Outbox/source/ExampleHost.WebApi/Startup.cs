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

using System.Reflection;
using Asp.Versioning;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.Core.Outbox.Extensions.DependencyInjection;
using ExampleHost.WebApi.DbContext;
using ExampleHost.WebApi.UseCases;
using ExampleHost.WebApi.UserCreatedEmailOutboxMessage;
using Microsoft.EntityFrameworkCore;

namespace ExampleHost.WebApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Add core web app services also required by the outbox (found in the DataHub.Core.App.WebApp package)
        services.AddNodaTimeForApplication();

        // Add services used for the example
        services.AddTransient<CreateUserService>();
        services.AddDbContext<MyApplicationDbContext>(dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseSqlServer(
                _configuration.GetConnectionString("ExampleHostDatabase"),
                o => o.UseNodaTime()); // UseNodaTime() is required (found in the SimplerSoftware.EntityFrameworkCore.SqlServer.NodaTime package)
        });

        // => Add services needed to add a message to the outbox
        // These are the only services needed if the application is only adding messages to the outbox, but the
        // outbox processing is running in a different application.
        services.AddOutboxClient<MyApplicationDbContext>();

        // => Add services needed for processing outbox messages (ie. publishing the messages)
        // These are the only services needed if the application is only processing messages from the outbox, not adding
        // new messages to the outbox
        services.AddTransient<IOutboxPublisher, UserCreatedEmailOutboxMessagePublisher>(); // The outbox message publishers specific to the application
        services.AddOutboxProcessor<MyApplicationDbContext>(); // The outbox processor that should run periodically in a background worker or a timer trigger

        // => Adding swagger is not required, but makes it easier to manually run/debug/test the API
        services
            .AddSwaggerForWebApp(
                executingAssembly: Assembly.GetExecutingAssembly(),
                swaggerUITitle: "ExampleHost.WebApi")
            .AddApiVersioningForWebApp(new ApiVersion(1));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // => Adding swagger is not required, but makes it easier to manually run/debug/test the API
        app.UseSwaggerForWebApp();
    }
}
