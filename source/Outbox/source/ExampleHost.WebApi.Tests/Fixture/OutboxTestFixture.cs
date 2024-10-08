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

using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.Outbox.Tests.IntegrationTests.Fixture;
using Energinet.DataHub.Core.Outbox.Tests.IntegrationTests.Fixture.Database;
using ExampleHost.WebApi.DbContext;
using Xunit;

namespace ExampleHost.WebApi.Tests.Fixture;

public class OutboxTestFixture : IAsyncLifetime
{
    public OutboxTestFixture()
    {
        DatabaseManager = new OutboxDatabaseManager<MyApplicationDbContext>();
        ExampleHostWebApiFactory = new ExampleHostWebApiFactory();
    }

    public OutboxDatabaseManager<MyApplicationDbContext> DatabaseManager { get; }

    [NotNull]
    public HttpClient? WebApiClient { get; private set; }

    private ExampleHostWebApiFactory ExampleHostWebApiFactory { get; }

    public async Task InitializeAsync()
    {
        await DatabaseManager.CreateDatabaseAsync();

        ExampleHostWebApiFactory.AppSettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:ExampleHostDatabase", DatabaseManager.ConnectionString },
        };

        WebApiClient = ExampleHostWebApiFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await DatabaseManager.DeleteDatabaseAsync();
        await ExampleHostWebApiFactory.DisposeAsync();
        WebApiClient.Dispose();
    }
}
