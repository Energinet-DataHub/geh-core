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

using Energinet.DataHub.Core.Outbox.Domain;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Core.Outbox.Infrastructure;

/// <summary>
/// Interface for a DbContext implementation that contains the Outbox table.
/// <remarks>
/// Requires <see cref="OutboxEntityConfiguration"/> to be applied to the DbContext configuration,
/// which usually happens in the <see cref="DbContext.OnModelCreating"/> method.
/// </remarks>
/// <example>Example of applying the <see cref="OutboxEntityConfiguration"/> in a <see cref="DbContext"/>:
/// <code>
/// protected override void OnModelCreating(ModelBuilder modelBuilder)
/// {
///    modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
/// }
/// </code>
/// </example>
/// </summary>
public interface IOutboxContext
{
    DbSet<OutboxMessage> Outbox { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
