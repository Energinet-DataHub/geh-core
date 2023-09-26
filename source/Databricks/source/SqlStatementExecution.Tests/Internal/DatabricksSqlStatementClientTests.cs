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

using System.Net;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests.Builders;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Microsoft.Extensions.Logging;
using Moq;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests.Internal;

public class DatabricksSqlStatementClientTests
{
    private readonly string _running;
    private readonly string _failed;
    private readonly string _closed;
    private readonly string _cancelled;
    private readonly string _pending;

    private readonly string _calculationResultChunkJson;
    private readonly string _calculationResultWithExternalLinks;
    private readonly string _chunkData;

    public DatabricksSqlStatementClientTests()
    {
        _calculationResultChunkJson = GetJsonFromFile("CalculationResultChunk.json");
        _calculationResultWithExternalLinks = GetJsonFromFile("CalculationResultWithExternalLinks.json");
        _chunkData = GetJsonFromFile("ChunkData.json");

        _running = SqlResponseStatusHelper.CreateStatusResponse("RUNNING");
        _failed = SqlResponseStatusHelper.CreateStatusResponse("FAILED");
        _closed = SqlResponseStatusHelper.CreateStatusResponse("CLOSED");
        _cancelled = SqlResponseStatusHelper.CreateStatusResponse("CANCELED");
        _pending = SqlResponseStatusHelper.CreateStatusResponse("PENDING");
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstRunningThenStatementFails_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(SqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_failed))
            .Returns(SqlResponse.CreateAsFailed(statementId));
        var sut = builder
            .AddHttpClientResponse(_running)
            .AddHttpClientResponse(_failed)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstRunningThenStatementIsClosed_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(SqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_closed))
            .Returns(SqlResponse.CreateAsClosed(statementId));
        var sut = builder
            .AddHttpClientResponse(_running)
            .AddHttpClientResponse(_closed)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstRunningThenStatementIsCancelled_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(SqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_cancelled))
            .Returns(SqlResponse.CreateAsCancelled(statementId));
        var sut = builder
            .AddHttpClientResponse(_running)
            .AddHttpClientResponse(_cancelled)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstPendingThenStatementFails_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_pending))
            .Returns(SqlResponse.CreateAsPending(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_failed))
            .Returns(SqlResponse.CreateAsFailed(statementId));
        var sut = builder
            .AddHttpClientResponse(_pending)
            .AddHttpClientResponse(_failed)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstPendingThenStatementIsClosed_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_pending))
            .Returns(SqlResponse.CreateAsPending(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_closed))
            .Returns(SqlResponse.CreateAsClosed(statementId));
        var sut = builder
            .AddHttpClientResponse(_pending)
            .AddHttpClientResponse(_closed)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstPendingThenStatementIsCancelled_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_pending))
            .Returns(SqlResponse.CreateAsPending(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_cancelled))
            .Returns(SqlResponse.CreateAsCancelled(statementId));
        var sut = builder
            .AddHttpClientResponse(_pending)
            .AddHttpClientResponse(_cancelled)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenHttpRequestFails_ThrowsDatabricksSqlException(DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        var sut = builder
            .AddHttpClientResponse("http request failed", HttpStatusCode.BadRequest)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenSecondHttpRequestFails_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<ISqlResponseParser> parserMock,
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(SqlResponse.CreateAsRunning(statementId));
        var sut = builder
            .AddHttpClientResponse(_running)
            .AddHttpClientResponse("http request failed", HttpStatusCode.BadRequest)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenMultipleChunks_GetAllChunks(
        DatabricksSqlStatementClientBuilder builder)
    {
        // Arrange
        var mockedLogger = new Mock<ILogger<SqlStatusResponseParser>>();
        var parser = new SqlResponseParser(
            new SqlStatusResponseParser(
                mockedLogger.Object,
                new SqlChunkResponseParser()),
            new SqlChunkResponseParser(),
            new SqlChunkDataResponseParser());

        var sut = builder
            .AddHttpClientResponse(_calculationResultWithExternalLinks)
            .AddExternalHttpClientResponse(_chunkData)
            .AddHttpClientResponse(_calculationResultChunkJson)
            .UseParser(parser)
            .Build();

        // Act
        var result = await sut.ExecuteAsync("some sql").ToListAsync();

        // Assert
        Assert.Equal(5, result.Count);
    }

    private string GetJsonFromFile(string fileName)
    {
        var stream = EmbeddedResources.GetStream(fileName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
