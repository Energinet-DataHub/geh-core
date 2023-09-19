// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using Energinet.DataHub.Core.Databricks.SqlStatementExecution;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests;

public class ServiceCollectionRegistrationTests
{
    [Fact]
    public void RegisteringDatabricks_Should_ReturnConfiguredHttpClient()
    {
        var services = new ServiceCollection();
        var workspaceUri = "https://foo.com";
        var workspaceToken = "bar";
        var warehouseId = "baz";

        services.AddDatabricks<SpySqlStatementClient>(warehouseId, workspaceToken, workspaceUri);

        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetRequiredService<ISqlStatementClient>();

        if (client is not SpySqlStatementClient spyClient)
        {
            throw new InvalidOperationException("Client is not of type SpySqlStatementClient");
        }

        Assert.True(spyClient.WorkspaceUriIs(new Uri(workspaceUri)));
        Assert.True(spyClient.WorkspaceTokenIs(workspaceToken));
    }

    [Fact]
    public void RegisteringDatabricks_Should_ResolveSqlClient()
    {
        var services = new ServiceCollection();
        var workspaceUri = "https://foo.com";
        var workspaceToken = "bar";
        var warehouseId = "baz";

        services.AddDatabricks<SpySqlStatementClient>(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetService<ISqlStatementClient>();

        client.Should().NotBeNull();
    }

    [Fact]
    public void OtherClass_Should_NotGetOurConfiguredClient()
    {
        var services = new ServiceCollection();
        var workspaceUri = "https://foo.com";
        var workspaceToken = "bar";
        var warehouseId = "baz";

        services.AddDatabricks<SpySqlStatementClient>(warehouseId, workspaceToken, workspaceUri);
        services.AddTransient<DependOnHttpClient>();

        var serviceProvider = services.BuildServiceProvider();

        var client = serviceProvider.GetRequiredService<DependOnHttpClient>();

        client.BaseAddressIsDefault().Should().BeTrue();
    }

    public class DependOnHttpClient
    {
        private readonly HttpClient _client;

        public DependOnHttpClient(HttpClient client)
        {
            _client = client;
        }

        public bool BaseAddressIsDefault()
        {
            return _client.BaseAddress == default;
        }
    }

    private sealed class SpySqlStatementClient : ISqlStatementClient
    {
        private readonly HttpClient _client;

        public SpySqlStatementClient(HttpClient client)
        {
            _client = client;
        }

        public IAsyncEnumerable<SqlResultRow> ExecuteAsync(string sqlStatement)
        {
            throw new NotImplementedException();
        }

        public bool WorkspaceUriIs(Uri workspaceUri)
        {
            return _client.BaseAddress == workspaceUri;
        }

        public bool WorkspaceTokenIs(string workspaceToken)
        {
            return
                _client.DefaultRequestHeaders.Authorization?.Parameter != null &&
                _client.DefaultRequestHeaders.Authorization?.Scheme == "Bearer" &&
                _client.DefaultRequestHeaders.Authorization.Parameter.EndsWith(workspaceToken);
        }
    }
}
