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
using ExampleHost.WebApi.DbContext;
using Microsoft.EntityFrameworkCore;

namespace ExampleHost.WebApi.Tests.Fixture;

public class OutboxDatabaseManager : SqlServerDatabaseManager<MyApplicationDbContext>
{
    public OutboxDatabaseManager()
        : base($"Outbox_{DateTime.Now:yyyyMMddHHmm}_")
    {
    }

    public override MyApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyApplicationDbContext>()
            .UseSqlServer(ConnectionString, options =>
            {
                options.UseNodaTime();
            });

        return (MyApplicationDbContext)Activator.CreateInstance(typeof(MyApplicationDbContext), optionsBuilder.Options)!;
    }

    public async Task TruncateOutboxTableAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Outbox]");
    }
}
