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
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.Internal.Publisher;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// that allow adding ServiceBus services to an application.
/// </summary>
public static class ServiceBusExtensions
{
    /// <summary>
    /// Register a <see cref="ServiceBusClient"/> to be used for the creation of subclients communicating
    /// within the configured ServiceBus namespace.
    /// </summary>
    public static IServiceCollection AddServiceBusClientForApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ServiceBusNamespaceOptions>()
            .BindConfiguration(ServiceBusNamespaceOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddAzureClients(builder =>
        {
            builder
                .UseCredential(new DefaultAzureCredential());

            var serviceBusNamespaceOptions =
                configuration
                    .GetRequiredSection(ServiceBusNamespaceOptions.SectionName)
                    .Get<ServiceBusNamespaceOptions>()
                ?? throw new InvalidOperationException("Missing ServiceBus Namespace configuration.");

            builder
                .AddServiceBusClientWithNamespace(serviceBusNamespaceOptions.FullyQualifiedNamespace);
        });

        return services;
    }

    /// <summary>
    /// Method for registering an integration events publisher.
    /// A <see cref="ServiceBusClient"/> must be registered first by calling <see cref="AddServiceBusClientForApplication"/>.
    /// It is the responsibility of the caller to register the dependencies of the <see cref="IIntegrationEventProvider"/> implementation.
    /// </summary>
    /// <typeparam name="TIntegrationEventProvider">The type of the service to use for outbound integration events.</typeparam>
    public static IServiceCollection AddIntegrationEventsPublisher<TIntegrationEventProvider>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TIntegrationEventProvider : class, IIntegrationEventProvider
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<IntegrationEventsOptions>()
            .BindConfiguration(IntegrationEventsOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddAzureClients(builder =>
        {
            var integrationEventsOptions =
                configuration
                    .GetRequiredSection(IntegrationEventsOptions.SectionName)
                    .Get<IntegrationEventsOptions>()
                ?? throw new InvalidOperationException("Missing Integration Events configuration.");

            builder
                .AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
                    provider
                        .GetRequiredService<ServiceBusClient>()
                        .CreateSender(integrationEventsOptions.TopicName))
                .WithName(integrationEventsOptions.TopicName);
        });

        services.AddScoped<IIntegrationEventProvider, TIntegrationEventProvider>();
        services.AddScoped<IPublisher, IntegrationEventsPublisher>();
        services.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();

        return services;
    }
}
