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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Database;
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Example.DatabaseMigration;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Core.Outbox.Tests.IntegrationTests.Fixture.Database;

/// <summary>
/// Manages databases created by the database migrations scripts in the Example.DatabaseMigration project.
/// </summary>
public class OutboxDatabaseManager<TDbContext> : SqlServerDatabaseManager<TDbContext>
    where TDbContext : DbContext, IOutboxContext
{
    public OutboxDatabaseManager()
        : base($"Outbox_{DateTime.Now:yyyyMMddHHmm}_")
    {
    }

    public override TDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlServer(ConnectionString, options =>
            {
                options.UseNodaTime();
            });

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }

    public async Task TruncateOutboxTableAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Outbox]");
    }

    /// <summary>
    /// Creates the database schema using DbUp instead of a database context.
    /// </summary>
    protected override Task<bool> CreateDatabaseSchemaAsync(TDbContext context)
    {
        return Task.FromResult(CreateDatabaseSchema(context));
    }

    /// <summary>
    /// Creates the database schema using DbUp instead of a database context.
    /// </summary>
    protected override bool CreateDatabaseSchema(TDbContext context)
    {
        var result = Upgrader.DatabaseUpgrade(ConnectionString);
        if (!result.Successful)
            throw new Exception("Database migration failed", result.Error);
        return true;
    }
}
