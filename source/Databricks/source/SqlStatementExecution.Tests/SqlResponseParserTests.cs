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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests;

[UnitTest]
public class SqlResponseParserTests
{
    private readonly string _succeededResultJson;
    private readonly string _pendingResultJson;
    private readonly string _runningResultJson;
    private readonly string _closedResultJson;
    private readonly string _canceledResultJson;
    private readonly string _failedResultJson;
    private readonly string _succeededResultStatementId;
    private readonly string[] _succeededResultColumnNames;

    public SqlResponseParserTests()
    {
        var stream = EmbeddedResources.GetStream("CalculationResult.json");
        using var reader = new StreamReader(stream);
        _succeededResultJson = reader.ReadToEnd();
        _succeededResultStatementId = "01edd9c4-2f88-1b8c-8764-bedad70547f2";
        _succeededResultColumnNames = new[]
        {
            "grid_area", "energy_supplier_id", "balance_responsible_id", "quantity", "quantity_quality", "time",
            "aggregation_level", "time_series_type", "batch_id", "batch_process_type", "batch_execution_time_start",
            "out_grid_area",
        };

        var chunkStream = EmbeddedResources.GetStream("CalculationResultChunk.json");
        using var chunkReader = new StreamReader(chunkStream);

        _pendingResultJson = SqlResponseStatusHelper.CreateStatusResponse("PENDING");
        _runningResultJson = SqlResponseStatusHelper.CreateStatusResponse("RUNNING");
        _closedResultJson = SqlResponseStatusHelper.CreateStatusResponse("CLOSED");
        _canceledResultJson = SqlResponseStatusHelper.CreateStatusResponse("CANCELED");
        _failedResultJson = SqlResponseStatusHelper.CreateStatusResponse("FAILED");
    }

    [Theory]
    [AutoMoqData]
    public void Parse_ReturnsResponseWithExpectedStatementId(
        SqlChunkResponse chunkResponse,
        [Frozen] Mock<ISqlChunkResponseParser> chunkParserMock,
        SqlStatusResponseParser sut)
    {
        // Arrange
        chunkParserMock.Setup(x => x.Parse(It.IsAny<JToken>())).Returns(chunkResponse);

        // Act
        var actual = sut.Parse(_succeededResultJson);

        // Assert
        actual.StatementId.Should().Be(_succeededResultStatementId);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_ReturnsResponseWithExpectedColumnNames(
        SqlChunkResponse chunkResponse,
        [Frozen] Mock<ISqlChunkResponseParser> chunkParserMock,
        SqlStatusResponseParser sut)
    {
        // Arrange
        chunkParserMock.Setup(x => x.Parse(It.IsAny<JToken>())).Returns(chunkResponse);

        // Act
        var actual = sut.Parse(_succeededResultJson);

        // Assert
        actual.ColumnNames.Should().BeEquivalentTo(_succeededResultColumnNames);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsPending_ReturnsResponseWithExpectedState(SqlStatusResponseParser sut)
    {
        // Arrange
        const SqlResponseState expectedState = SqlResponseState.Pending;

        // Act
        var actual = sut.Parse(_pendingResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [InlineAutoMoqData]
    public void Parse_WhenStateIsSucceeded_ReturnsResponseWithExpectedState(
        SqlChunkResponse chunkResponse,
        [Frozen] Mock<ISqlChunkResponseParser> chunkParserMock,
        SqlStatusResponseParser sut)
    {
        // Arrange
        const SqlResponseState expectedState = SqlResponseState.Succeeded;
        chunkParserMock.Setup(x => x.Parse(It.IsAny<JToken>())).Returns(chunkResponse);

        // Act
        var actual = sut.Parse(_succeededResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsCanceled_ReturnsResponseWithExpectedState(SqlStatusResponseParser sut)
    {
        // Arrange
        const SqlResponseState expectedState = SqlResponseState.Cancelled;

        // Act
        var actual = sut.Parse(_canceledResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsRunning_ReturnsResponseWithExpectedState(SqlStatusResponseParser sut)
    {
        // Arrange
        const SqlResponseState expectedState = SqlResponseState.Running;

        // Act
        var actual = sut.Parse(_runningResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsClosed_ReturnsResponseWithExpectedState(SqlStatusResponseParser sut)
    {
        // Arrange
        const SqlResponseState expectedState = SqlResponseState.Closed;

        // Act
        var actual = sut.Parse(_closedResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsUnknown_LogsErrorAndThrowsDatabricksSqlException(
        [Frozen] Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        SqlStatusResponseParser sut)
    {
        // Arrange
        var resultJson = SqlResponseStatusHelper.CreateStatusResponse("UNKNOWN");

        // Act and assert
        Assert.Throws<SqlException>(() => sut.Parse(resultJson));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenStateIsFailed_ReturnsResponseWithExpectedState(SqlStatusResponseParser sut)
    {
        // Arrange
        const SqlResponseState expectedState = SqlResponseState.Failed;

        // Act
        var actual = sut.Parse(_failedResultJson);

        // Assert
        actual.State.Should().Be(expectedState);
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenValidJson_ReturnsResult(SqlStatusResponseParser sut)
    {
        // Arrange
        var statementId = new JProperty("statement_id", Guid.NewGuid());
        var status = new JProperty("status", new JObject(new JProperty("state", "PENDING")));
        var manifest = new JProperty("manifest", new JObject(new JProperty("schema", new JObject(new JProperty("columns", new JArray(new JObject(new JProperty("name", "grid_area"))))))));
        var result = new JProperty("result", new JObject(new JProperty("data_array", new List<string[]>())));
        var obj = new JObject(statementId, status, manifest, result);
        var jsonString = obj.ToString();

        // Act + Assert
        sut.Parse(jsonString).Should().NotBeNull();
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenInvalidJson_ThrowsException(
        SqlStatusResponseParser sut)
    {
        // Arrange
        var statementId = new JProperty("statement_id", Guid.NewGuid());
        var status = new JProperty("not_status", new JObject(new JProperty("state", "PENDING")));
        var manifest = new JProperty("manifest", new JObject(new JProperty("schema", new JObject(new JProperty("columns", new JArray(new JObject(new JProperty("name", "grid_area"))))))));
        var result = new JProperty("result", new JObject(new JProperty("data_array", new List<string[]>())));
        var obj = new JObject(statementId, status, manifest, result);
        var jsonString = obj.ToString();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => sut.Parse(jsonString));
    }

    [Theory]
    [AutoMoqData]
    public void Parse_WhenInvalidJsonWithErrorCode_LogsErrorAndThrowsDatabricksSqlException(
        [Frozen] Mock<ILogger<SqlStatusResponseParser>> loggerMock,
        SqlStatusResponseParser sut)
    {
        var errorCode = new JProperty("error_code", "NOT_FOUND");
        var errorMessage = new JProperty("error_message", "Statement not found");
        var details = new JProperty("details", new JObject(new JProperty("description", "does not exist")));
        var obj = new JObject(errorCode, errorMessage, details);
        var jsonString = obj.ToString();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => sut.Parse(jsonString));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
