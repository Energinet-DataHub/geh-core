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

using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.Core.Outbox.Application;
using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.Outbox.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Outbox.Extensions.DependencyInjection;

public static class OutboxExtensions
{
    /// <summary>
    /// Add services required for creating and persisting outbox messages.
    /// <remarks>Requires <see cref="IOutboxContext"/> to be registered in service collection.</remarks>
    /// </summary>
    public static IServiceCollection AddOutboxModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // services.AddTransient<IDataRetention, OutboxRetention>(); TODO: Data retention in shared package?
        // services.AddNodaTimeForApplication(); //  TODO: Add NodaTime in shared package?
        services.AddTransient<IOutboxRepository, OutboxRepository>();
        services.AddTransient<IOutboxClient, OutboxClient>();

        return services;
    }

    /// <summary>
    /// Add services required for processing and publishing outbox messages.
    /// <remarks>Requires <see cref="AddOutboxModule"/> to be registered as well</remarks>
    /// </summary>
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services)
    {
        services.AddTransient<IOutboxProcessor, OutboxProcessor>();

        return services;
    }
}
