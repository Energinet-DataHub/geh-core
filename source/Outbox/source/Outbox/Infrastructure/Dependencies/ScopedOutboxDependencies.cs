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

using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Outbox.Infrastructure.Dependencies;

internal sealed record ScopedOutboxDependencies : IScopedOutboxDependencies
{
    private readonly IServiceScope _serviceScope;

    public ScopedOutboxDependencies(IServiceScopeFactory factory)
    {
        _serviceScope = factory.CreateScope();

        OutboxContext = _serviceScope.ServiceProvider.GetRequiredService<IOutboxContext>();
        OutboxRepository = _serviceScope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        OutboxPublishers = _serviceScope.ServiceProvider.GetServices<IOutboxPublisher>();
    }

    public IOutboxContext OutboxContext { get; }

    public IOutboxRepository OutboxRepository { get; }

    public IEnumerable<IOutboxPublisher> OutboxPublishers { get; }

    public void Dispose()
    {
        // Disposing on service scope will dispose all the dependencies (like OutboxContext)
        _serviceScope.Dispose();
    }
}
