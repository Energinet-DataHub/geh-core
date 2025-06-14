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

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.Builder;
using Energinet.DataHub.Core.App.FunctionApp.Extensions.DependencyInjection;
using ExampleHost.FunctionApp01.Common;
using ExampleHost.FunctionApp01.Extensions.DependencyInjection;
using ExampleHost.FunctionApp01.Functions;
using ExampleHost.FunctionApp01.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

var host = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        // Configuration verified in tests:
        //  * Logging using ILogger<T> will work, but notice that by default we need to log as "Warning" for it to
        //    appear in Application Insights (can be configured).
        //  * We can see Trace, Request, Dependencies and other entries in App Insights out-of-box.
        //  * Telemetry events are enriched with property "Subsystem" and configured value
        services.AddApplicationInsightsForIsolatedWorker(subsystemName: "ExampleHost.FunctionApp");

        // Configure token credential provider to share token credential
        //  * Used by "AddAuthorizationHeaderProvider"
        //  * Can be used when registering access to Azure resources (e.g. service bus etc.)
        services.AddTokenCredentialProvider();

        // Configure ServiceBusSender for calling FunctionApp02
        services.AddSingleton(sp =>
        {
            var serviceBusFullyQualifiedNamespace = Environment.GetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventFullyQualifiedNamespace);
            var credential = sp.GetRequiredService<TokenCredentialProvider>().Credential;
            return new ServiceBusClient(serviceBusFullyQualifiedNamespace, credential);
        });
        services.AddSingleton<ServiceBusSender>(sp =>
        {
            var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
            var topicName = Environment.GetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventTopicName);
            return serviceBusClient.CreateSender(topicName);
        });

        // Health Checks (verified in tests)
        services.AddHealthChecksForIsolatedWorker();
        services
            .AddHealthChecks()
            .AddCheck("verify-ready", () => HealthCheckResult.Healthy())
            .AddCheck("verify-status", () => HealthCheckResult.Healthy(), tags: [HealthChecksConstants.StatusHealthCheckTag]);

        // Http => Authentication using DarkLoop Authorization extension (verified in tests)
        services
            .AddJwtBearerAuthenticationForIsolatedWorker(context.Configuration)
            .AddUserAuthenticationForIsolatedWorker<ExampleSubsystemUser, ExampleSubsystemUserProvider>();

        // Feature management (verified in tests)
        //  * Must call "AddAzureAppConfiguration" before "UseAzureAppConfigurationForIsolatedWorker"
        services
            .AddAzureAppConfiguration()
            .AddFeatureManagement();

        // Http => Client side subsystem-to-subsystem authentication (verified in tests)
        //  * Depends on services registered by "AddTokenCredentialProvider"
        services
            .AddAuthorizationHeaderProvider()
            .AddApp02HttpClient();
    })
    .ConfigureFunctionsWebApplication(builder =>
    {
        // Configuration verified in tests:
        //  * Enable automatic feature flag refresh on each function execution (except for DF Orchestration triggers)
        //  * Must be called after "AddAzureAppConfiguration" as it verifies if services was registered
        builder.UseAzureAppConfigurationForIsolatedWorker();

        // DarkLoop Authorization extension (verified in tests):
        //  * Explicitly adding the extension middleware because registering middleware when extension is loaded does not
        //    place the middleware in the pipeline where required request information is available.
        builder.UseFunctionsAuthorization();

        // Configuration verified in tests:
        //  * Endpoints for which UserMiddleware is enabled must call the endpoint with a token
        //  * We exclude endpoints for which we in tests do not want to, or cannot, send a token
        builder.UseUserMiddlewareForIsolatedWorker<ExampleSubsystemUser>(
            excludedFunctionNames: [
                $"{nameof(RestApiExampleFunction.TelemetryAsync)}",
                $"{nameof(FeatureManagementFunction.GetMessage)}",
                $"{nameof(FeatureManagementFunction.CreateMessage)}",
                $"{nameof(FeatureManagementFunction.GetFeatureFlagState)}",
                $"{nameof(DurableFunction.ExecuteDurableFunction)}",
                $"{nameof(SubsystemAuthenticationFunction.GetWithPermissionForSubsystemAsync)}"]);
    })
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        // Configuration verified in tests:
        //  * Only load feature flags from App Configuration
        //  * Use default refresh interval of 30 seconds
        configBuilder.AddAzureAppConfigurationForIsolatedWorker();
    })
    .ConfigureLogging((context, logging) =>
    {
        // Configuration verified in tests:
        //  * Ensure Application Insights logging configuration is picked up.
        logging.AddLoggingConfigurationForIsolatedWorker(context.Configuration);
    })
    .Build();

host.Run();
