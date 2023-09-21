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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Constants;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests;

public class SqlStatementExecutionExtensionsTests
{
    [Fact]
    public void Deprecate_AddDatabricks_Should_ReturnSqlStatementClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddSqlStatementExecution(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetRequiredService<IDatabricksSqlStatementClient>();

        using var assertionScope = new AssertionScope();
        client.Should().BeOfType<DatabricksSqlStatementClient>();
    }

    [Fact]
    public void AddSqlStatementExecution_Should_ReturnConfiguredHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddSqlStatementExecution<SpyDatabricksSqlStatementClient>(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetRequiredService<IDatabricksSqlStatementClient>();

        using var assertionScope = new AssertionScope();
        var spyClient = client.Should().BeOfType<SpyDatabricksSqlStatementClient>().Subject;
        spyClient.WorkspaceUriIs(new Uri(workspaceUri)).Should().BeTrue();
        spyClient.WorkspaceTokenIs(workspaceToken).Should().BeTrue();
    }

    [Fact]
    public void AddSqlStatementExecution_Should_ReturnConfiguredExternalHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddSqlStatementExecution<SpyDatabricksSqlStatementClient>(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetRequiredService<IDatabricksSqlStatementClient>();

        using var assertionScope = new AssertionScope();
        var spyClient = client.Should().BeOfType<SpyDatabricksSqlStatementClient>().Subject;
        spyClient.ExternalClientIsConfigured().Should().BeTrue();
    }

    [Fact]
    public void AddSqlStatementExecution_Should_ResolveSqlClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddSqlStatementExecution<SpyDatabricksSqlStatementClient>(warehouseId, workspaceToken, workspaceUri);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IDatabricksSqlStatementClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void OtherClass_Should_NotGetOurConfiguredClient()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceUri = "https://foo.com";
        const string workspaceToken = "bar";
        const string warehouseId = "baz";

        // Act
        services.AddSqlStatementExecution<SpyDatabricksSqlStatementClient>(warehouseId, workspaceToken, workspaceUri);
        services.AddTransient<DependOnHttpClient>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
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

    private sealed class SpyDatabricksSqlStatementClient : IDatabricksSqlStatementClient
    {
        private readonly HttpClient _client;
        private readonly HttpClient _externalClient;

        public SpyDatabricksSqlStatementClient(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient(HttpClientNameConstants.Databricks);
            _externalClient = httpClientFactory.CreateClient(HttpClientNameConstants.External);
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

        public bool ExternalClientIsConfigured()
        {
            return _externalClient.BaseAddress == default &&
                   _externalClient.DefaultRequestHeaders.Authorization == null;
        }
    }
}
