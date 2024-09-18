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
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Energinet.DataHub.Core.Outbox.Infrastructure.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Energinet.DataHub.Core.Outbox.Extensions.DependencyInjection;

public static class OutboxExtensions
{
    /// <summary>
    /// Add services required for creating and persisting outbox messages.
    /// <remarks>
    /// Requires an <see cref="IOutboxContext"/> to be registered in the service collection. See the
    /// <see cref="IOutboxContext"/> documentation for more information.
    /// </remarks>
    /// </summary>
    public static IServiceCollection AddOutboxClient<TDbContext>(this IServiceCollection services)
        where TDbContext : IOutboxContext
    {
        AddSharedDependencies<TDbContext>(services);

        services.AddTransient<IOutboxClient, OutboxClient>();

        return services;
    }

    /// <summary>
    /// Add services required for processing and publishing outbox messages.
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Requires an <see cref="IOutboxContext"/> to be registered in the service collection. See the
    /// <see cref="IOutboxContext"/> documentation for more information.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Requires <see cref="IOutboxPublisher"/>'s for each type of <see cref="IOutboxMessage{TPayload}"/> to be
    /// registered in the service collection. See the <see cref="IOutboxContext"/> documentation for more information.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// </summary>
    public static IServiceCollection AddOutboxProcessor<TDbContext>(this IServiceCollection services)
        where TDbContext : IOutboxContext
    {
        AddSharedDependencies<TDbContext>(services);
        services.AddTransient<IOutboxScopeFactory, OutboxScopeFactory>();
        services.AddTransient<IOutboxProcessor, OutboxProcessor>();

        return services;
    }

    private static void AddSharedDependencies<TDbContext>(IServiceCollection services)
        where TDbContext : IOutboxContext
    {
        services.TryAddTransient<IOutboxContext>(sc => sc.GetRequiredService<TDbContext>());
        services.TryAddTransient<IOutboxRepository, OutboxRepository>();
    }
}
