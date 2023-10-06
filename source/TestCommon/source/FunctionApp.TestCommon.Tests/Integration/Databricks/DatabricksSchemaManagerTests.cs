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

using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Databricks;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Databricks;

public class DatabricksSchemaManagerTests
{
    [Fact]
    public async Task When_CreateSchemaThenSchemaExist()
    {
        // Arrange
        var sut = new DatabricksSchemaManagerSut(new DatabricksSettings(), "test-common");

        // Act
        await sut.CreateSchemaAsync();

        // Assert
        sut.SchemaExists.Should().BeTrue();
        sut.SchemaName.Should().NotBeEmpty();
        sut.ExecuteSqlAsyncHasExecuted.Should().Be(1);
    }

    [Fact]
    public async Task When_DropSchemaThenSchemaDoesNotExist()
    {
        // Arrange
        var sut = new DatabricksSchemaManagerSut(new DatabricksSettings(), "test-common");

        // Act
        await sut.DropSchemaAsync();

        // Assert
        sut.SchemaExists.Should().BeFalse();
        sut.ExecuteSqlAsyncHasExecuted.Should().Be(1);
    }
}

public class DatabricksSchemaManagerSut : DatabricksSchemaManager
{
    private int _executeSqlAsyncHasExecuted;

    public DatabricksSchemaManagerSut(DatabricksSettings databricksSettings, string schemaPrefix)
        : base(databricksSettings, schemaPrefix)
    {
        _executeSqlAsyncHasExecuted = 0;
    }

    public int ExecuteSqlAsyncHasExecuted => _executeSqlAsyncHasExecuted;

    protected override async Task ExecuteSqlAsync(string sqlStatement)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        _executeSqlAsyncHasExecuted += 1;
    }

    protected override HttpClient CreateHttpClient(DatabricksSettings databricksOptions)
    {
        return new HttpClient();
    }
}
