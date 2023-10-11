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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Databricks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Databricks;

public sealed class DatabricksSchemaManagerTests
{
    [Fact]
    public async Task WhenCreateSchemaThenSchemaExistAsync()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string expectedCommand = "CREATE SCHEMA";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.CreateSchemaAsync();

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
        sut.SchemaExists.Should().BeTrue();
        sut.SchemaName.Should().Contain(schemaPrefix);
    }

    [Fact]
    public async Task WhenDropSchemaThenSchemaDoesNotExistAsync()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string expectedCommand = "DROP SCHEMA";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.DropSchemaAsync();

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
        sut.SchemaExists.Should().BeFalse();
    }

    [Fact]
    public async Task WhenCreateTableThenTableIsCreatedAsync()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string expectedCommand = "CREATE TABLE";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var columns = new Dictionary<string, (string DataType, bool IsNullable)> { { "id", ("int", false) } };
        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.CreateTableAsync("test-table", columns);

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
    }

    [Fact]
    public async Task WhenInsertRowsThenSeveralRowsAreInserted()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string tableName = "test-table";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var rows = new List<List<string>> { new() { "1", "a" }, new() { "2", "b" } };
        const string expectedRowStr = "(1, a), (2, b)";
        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.InsertAsync("test-table", rows);

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        var expectedCommand = $"INSERT INTO {sut.SchemaName}.{tableName} VALUES {expectedRowStr}";

        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
    }

    [Fact]
    public async Task WhenInsertRowsWithColumnNameArgsThenSeveralRowsAreInserted()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string tableName = "test-table";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var columns = new List<string> { "id", "name" };
        const string expectedColumnStr = "(id, name)";
        var rows = new List<List<string>> { new() { "1", "a" }, new() { "2", "b" } };
        const string expectedRowStr = "(1, a), (2, b)";

        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.InsertAsync("test-table", columns, rows);

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        var expectedCommand = $"INSERT INTO {sut.SchemaName}.{tableName} {expectedColumnStr} VALUES {expectedRowStr}";

        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
    }

    [Fact]
    public async Task WhenInsertThenSingleRowIsInserted()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string tableName = "test-table";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var rows = new List<string> { "1", "a" };
        const string expectedRowStr = "(1, a)";
        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.InsertAsync("test-table", rows);

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        var expectedCommand = $"INSERT INTO {sut.SchemaName}.{tableName} VALUES {expectedRowStr}";

        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
    }

    [Fact]
    public async Task WhenInsertWithColumnsArgThenSingleRowIsInserted()
    {
        // Arrange
        const string schemaPrefix = "test-common";
        const string tableName = "test-table";
        var mockHandler = new MockHttpMessageHandler();
        var mockHttpClientFactory = CreateHttpClientFactoryMock(mockHandler);

        var columns = new List<string> { "id", "name" };
        const string expectedColumnStr = "(id, name)";
        var rows = new List<string> { "1", "a" };
        const string expectedRowStr = "(1, a)";
        var sut =
            new DatabricksSchemaManager(mockHttpClientFactory.Object, new DatabricksSettings(), schemaPrefix);

        // Act
        await sut.InsertAsync("test-table", columns, rows);

        // Assert
        mockHttpClientFactory.Verify(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>()), Times.Once);
        mockHandler.LastRequest.Should().NotBeNull();
        var expectedCommand = $"INSERT INTO {sut.SchemaName}.{tableName} {expectedColumnStr} VALUES {expectedRowStr}";

        (await mockHandler.LastRequest!.Content!.ReadAsStringAsync()).Should().Contain(expectedCommand);
    }

    private Mock<IHttpClientFactory> CreateHttpClientFactoryMock(MockHttpMessageHandler mockHandler)
    {
        var mockHttpClient = new HttpClient(mockHandler);
        mockHttpClient.BaseAddress = new Uri("https://test");
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();

        mockHttpClientFactory
        .Setup(f => f.CreateHttpClient(It.IsAny<DatabricksSettings>())).Returns(mockHttpClient);
        return mockHttpClientFactory;
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage? request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"status\": { \"state\": \"SUCCEEDED\" }," +
                                        " \"response\": \"test\" }"),
        };

        return await Task.FromResult(responseMessage);
    }
}
