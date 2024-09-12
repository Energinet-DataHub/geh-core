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
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace ExampleHost.WebApi.DbContext;

/// <summary>
/// The application database context must implement the interface <see cref="IOutboxContext"/> and
/// add the <see cref="OutboxEntityConfiguration"/> in the <see cref="OnModelCreating"/> method.
/// <remarks>
/// An example script of creating the outbox table through dbup can be seen at: (TODO: INSERT URL TO DOCS)
/// </remarks>
/// </summary>
public class MyApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext, IOutboxContext
{
    public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<OutboxMessage> Outbox { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The outbox entity configuration must be added to the model builder to correctly configure the outbox table.
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
    }
}
