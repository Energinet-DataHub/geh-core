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

using System.Net;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests.Builders;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Moq;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecutionTests;

public class SqlStatementClientTests
{
    private readonly string _running;
    private readonly string _failed;
    private readonly string _closed;
    private readonly string _cancelled;
    private readonly string _pending;

    public SqlStatementClientTests()
    {
        _running = DatabrickSqlResponseStatusHelper.CreateStatusResponse("RUNNING");
        _failed = DatabrickSqlResponseStatusHelper.CreateStatusResponse("FAILED");
        _closed = DatabrickSqlResponseStatusHelper.CreateStatusResponse("CLOSED");
        _cancelled = DatabrickSqlResponseStatusHelper.CreateStatusResponse("CANCELED");
        _pending = DatabrickSqlResponseStatusHelper.CreateStatusResponse("PENDING");
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ExecuteAsync_WhenFirstRunningThenStatementFails_ThrowsDatabricksSqlException(
        Guid statementId,
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(DatabricksSqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_failed))
            .Returns(DatabricksSqlResponse.CreateAsFailed(statementId));
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
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(DatabricksSqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_closed))
            .Returns(DatabricksSqlResponse.CreateAsClosed(statementId));
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
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(DatabricksSqlResponse.CreateAsRunning(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_cancelled))
            .Returns(DatabricksSqlResponse.CreateAsCancelled(statementId));
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
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_pending))
            .Returns(DatabricksSqlResponse.CreateAsPending(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_failed))
            .Returns(DatabricksSqlResponse.CreateAsFailed(statementId));
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
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_pending))
            .Returns(DatabricksSqlResponse.CreateAsPending(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_closed))
            .Returns(DatabricksSqlResponse.CreateAsClosed(statementId));
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
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_pending))
            .Returns(DatabricksSqlResponse.CreateAsPending(statementId));
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_cancelled))
            .Returns(DatabricksSqlResponse.CreateAsCancelled(statementId));
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
    public async Task ExecuteAsync_WhenHttpRequestFails_ThrowsDatabricksSqlException(SqlStatementClientBuilder builder)
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
        Mock<IDatabricksSqlResponseParser> parserMock,
        SqlStatementClientBuilder builder)
    {
        // Arrange
        parserMock
            .Setup(parser => parser.ParseStatusResponse(_running))
            .Returns(DatabricksSqlResponse.CreateAsRunning(statementId));
        var sut = builder
            .AddHttpClientResponse(_running)
            .AddHttpClientResponse("http request failed", HttpStatusCode.BadRequest)
            .UseParser(parserMock.Object)
            .Build();

        // Act and assert
        await Assert.ThrowsAsync<DatabricksSqlException>(async () => await sut.ExecuteAsync("some sql").ToListAsync());
    }
}