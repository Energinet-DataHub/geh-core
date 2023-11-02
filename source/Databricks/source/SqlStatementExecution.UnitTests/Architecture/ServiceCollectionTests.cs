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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Architecture;

public class ServiceCollectionTests
{
    [Fact]
    public void CanResolve_DatabricksSqlWarehouseQueryExecutor_FromServiceCollection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WorkspaceUrl"] = "https://foo.com",
                ["WarehouseId"] = "baz",
                ["WorkspaceToken"] = "bar",
            })
            .Build();

        // Act
        var services = new ServiceCollection();
        services.AddDatabricksSqlStatementExecution(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var svc = serviceProvider.GetService<DatabricksSqlWarehouseQueryExecutor>();

        // Assert
        svc.Should().NotBeNull();
    }

    [Fact]
    public void CanResolve_IDatabricksStatementExecutor_FromServiceCollection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WorkspaceUrl"] = "https://foo.com",
                ["WarehouseId"] = "baz",
                ["WorkspaceToken"] = "bar",
            })
            .Build();

        // Act
        var services = new ServiceCollection();
        services.AddDatabricksSqlStatementExecution(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var svc = serviceProvider.GetService<IDatabricksSqlWarehouseQueryExecutor>();

        // Assert
        svc.Should().NotBeNull();
    }
}
