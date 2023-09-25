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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests;

[UnitTest]
public class SqlResponseTests
{
    private readonly string[] _columnNames = { "someColumn" };
    private readonly SqlChunkResponse _chunkResponse = new(new Uri("https://foo.com"), "bar");

    [Theory]
    [AutoMoqData]
    public void CreateAsPending_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = SqlResponse.CreateAsPending(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(SqlResponseState.Pending);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsRunning_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = SqlResponse.CreateAsRunning(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(SqlResponseState.Running);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsSucceeded_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = SqlResponse.CreateAsSucceeded(statementId, _columnNames, _chunkResponse);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeEquivalentTo(_columnNames);
        actual.Chunk.Should().BeEquivalentTo(_chunkResponse);
        actual.State.Should().Be(SqlResponseState.Succeeded);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsFailed_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = SqlResponse.CreateAsFailed(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(SqlResponseState.Failed);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsCanceled_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = SqlResponse.CreateAsCancelled(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(SqlResponseState.Cancelled);
    }

    [Theory]
    [AutoMoqData]
    public void CreateAsClosed_ReturnsResponseWithExpectedProperties(Guid statementId)
    {
        // Act
        var actual = SqlResponse.CreateAsClosed(statementId);

        // Assert
        actual.StatementId.Should().Be(statementId);
        actual.ColumnNames.Should().BeNull();
        actual.Chunk.Should().BeNull();
        actual.State.Should().Be(SqlResponseState.Closed);
    }
}
