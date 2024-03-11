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

using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Azurite;

public class AzuriteManagerTests
{
    /// <summary>
    /// Only one Azurite process can be running at the same time.
    /// But since we have set 'DisableTestParallelization' to 'true'
    /// no test in current test assembly is executed in parallel,
    /// and as such we don't have to use collection fixtures here.
    /// </summary>
    public class VerifyAzuriteCanBeStartedTwice
    {
        [Fact]
        public async Task When_AzuriteProcessIsDisposed_Then_ItCanStartAgain()
        {
            // Arrange
            var azuriteManagerToStartFirst = new AzuriteManager();
            try
            {
                azuriteManagerToStartFirst.StartAzurite();
            }
            finally
            {
                // Act
                azuriteManagerToStartFirst.Dispose();
            }

            // Assert
            var azuriteManagerToStartSecond = new AzuriteManager();
            try
            {
                azuriteManagerToStartSecond.StartAzurite();
                var exception = await Record.ExceptionAsync(CreateStorageContainerAsync);
                exception.Should().BeNull();
            }
            finally
            {
                azuriteManagerToStartSecond.Dispose();
            }
        }

        [Fact]
        public void When_AzuriteProcessIsNotDisposed_Then_ItCanStillStartAgain()
        {
            // Arrange
            var azuriteManagerToStartFirst = new AzuriteManager();
            var azuriteManagerToStartSecond = new AzuriteManager();
            try
            {
                azuriteManagerToStartFirst.StartAzurite();

                // Act
                var exception = Record.Exception(() => azuriteManagerToStartSecond.StartAzurite());

                // Assert
                exception.Should().BeNull();
            }
            finally
            {
                azuriteManagerToStartFirst.Dispose();
                azuriteManagerToStartSecond.Dispose();
            }
        }

        private static async Task CreateStorageContainerAsync()
        {
            var storageConnectionString = "UseDevelopmentStorage=true";
            var containerName = $"Test{Guid.NewGuid()}".ToLower();

            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateAsync();
        }
    }

    /// <summary>
    /// When using Azurite with OAuth we must use Https and 'localhost' (not '127.0.0.1').
    ///
    /// We use a class fixture to ensure we can use this same AzuriteManager instance
    /// for multiple tests, to avoid starting and disposing Azurite many times.
    /// </summary>
    public class Given_OAuthIsTrue : IClassFixture<AzuriteManagerFixture>
    {
        public Given_OAuthIsTrue(AzuriteManagerFixture fixture)
        {
            Fixture = fixture;
            Fixture.StartAzuriteOnce(useOAuth: true);
        }

        private AzuriteManagerFixture Fixture { get; }

        [Fact]
        public async Task When_BlobServiceClient_UseDevelopmentStorageShortcut_Then_CreateContainerShouldFail()
        {
            // Arrange
            var client = new BlobServiceClient(
                connectionString: "UseDevelopmentStorage=true",
                CreateBlobNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

            // Assert
            exception.Should().NotBeNull();
        }

        [Fact]
        public async Task When_BlobServiceClient_UsingConnectionString_Then_CanCreateContainer()
        {
            // Arrange
            var client = new BlobServiceClient(
                connectionString: Fixture.AzuriteManager!.BlobStorageConnectionString,
                CreateBlobNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_BlobServiceClient_UsingHttpsAndTokenCredential_Then_CanCreateContainer()
        {
            // Arrange
            var client = new BlobServiceClient(
                serviceUri: Fixture.AzuriteManager!.BlobStorageServiceUri,
                credential: new DefaultAzureCredential(),
                CreateBlobNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_QueueServiceClient_UseDevelopmentStorageShortcut_Then_CreateQueueShouldFail()
        {
            // Arrange
            var client = new QueueServiceClient(
                connectionString: "UseDevelopmentStorage=true",
                CreateQueueNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

            // Assert
            exception.Should().NotBeNull();
        }

        [Fact]
        public async Task When_QueueServiceClient_UsingConnectionString_Then_CanCreateQueue()
        {
            // Arrange
            var client = new QueueServiceClient(
                connectionString: Fixture.AzuriteManager!.QueueStorageConnectionString,
                CreateQueueNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_QueueServiceClient_UsingHttpsAndTokenCredential_Then_CanCreateQueue()
        {
            // Arrange
            var client = new QueueServiceClient(
                serviceUri: Fixture.AzuriteManager!.QueueStorageServiceUri,
                credential: new DefaultAzureCredential(),
                CreateQueueNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_TableServiceClient_UseDevelopmentStorageShortcut_Then_CreateTableShouldFail()
        {
            // Arrange
            var client = new TableServiceClient(
                connectionString: "UseDevelopmentStorage=true",
                CreateTableNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateTableAsync(client));

            // Assert
            exception.Should().NotBeNull();
        }

        [Fact]
        public async Task When_TableServiceClient_UsingConnectionString_Then_CanCreateTable()
        {
            // Arrange
            var client = new TableServiceClient(
                connectionString: Fixture.AzuriteManager!.TableStorageConnectionString,
                CreateTableNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateTableAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_TableServiceClient_UsingHttpsAndTokenCredential_Then_CanCreateTable()
        {
            // Arrange
            var client = new TableServiceClient(
                endpoint: Fixture.AzuriteManager!.TableStorageServiceUri,
                tokenCredential: new DefaultAzureCredential(),
                CreateTableNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateTableAsync(client));

            // Assert
            exception.Should().BeNull();
        }
    }

    /// <summary>
    /// We use a class fixture to ensure we can use this same AzuriteManager instance
    /// for multiple tests, to avoid starting and disposing Azurite many times.
    /// </summary>
    public class Given_OAuthIsFalse : IClassFixture<AzuriteManagerFixture>
    {
        public Given_OAuthIsFalse(AzuriteManagerFixture fixture)
        {
            Fixture = fixture;
            Fixture.StartAzuriteOnce(useOAuth: false);
        }

        private AzuriteManagerFixture Fixture { get; }

        [Fact]
        public async Task When_BlobServiceClient_UseDevelopmentStorageShortcut_Then_CanCreateContainer()
        {
            // Arrange
            var client = new BlobServiceClient(
                connectionString: "UseDevelopmentStorage=true",
                CreateBlobNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_BlobServiceClient_UsingConnectionString_Then_CanCreateContainer()
        {
            // Arrange
            var client = new BlobServiceClient(
                connectionString: Fixture.AzuriteManager!.BlobStorageConnectionString,
                CreateBlobNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateBlobContainerAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_QueueServiceClient_UseDevelopmentStorageShortcut_Then_CanCreateQueue()
        {
            // Arrange
            var client = new QueueServiceClient(
                connectionString: "UseDevelopmentStorage=true",
                CreateQueueNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_QueueServiceClient_UsingConnectionString_Then_CanCreateQueue()
        {
            // Arrange
            var client = new QueueServiceClient(
                connectionString: Fixture.AzuriteManager!.QueueStorageConnectionString,
                CreateQueueNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateQueueAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_TableServiceClient_UseDevelopmentStorageShortcut_Then_CanCreateTable()
        {
            // Arrange
            var client = new TableServiceClient(
                connectionString: "UseDevelopmentStorage=true",
                CreateTableNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateTableAsync(client));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task When_TableServiceClient_UsingConnectionString_Then_CanCreateTable()
        {
            // Arrange
            var client = new TableServiceClient(
                connectionString: Fixture.AzuriteManager!.TableStorageConnectionString,
                CreateTableNoRetryOptions());

            // Act
            var exception = await Record.ExceptionAsync(() => CreateTableAsync(client));

            // Assert
            exception.Should().BeNull();
        }
    }

    private static BlobClientOptions CreateBlobNoRetryOptions()
    {
        var retryOptions = new BlobClientOptions();
        retryOptions.Retry.MaxRetries = 0;

        return retryOptions;
    }

    private static Task CreateBlobContainerAsync(BlobServiceClient blobServiceClient)
    {
        var containerName = $"Test{Guid.NewGuid()}".ToLower();

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        return blobContainerClient.CreateAsync();
    }

    private static QueueClientOptions CreateQueueNoRetryOptions()
    {
        var retryOptions = new QueueClientOptions();
        retryOptions.Retry.MaxRetries = 0;

        return retryOptions;
    }

    private static Task CreateQueueAsync(QueueServiceClient queueServiceClient)
    {
        var queueName = $"Test{Guid.NewGuid()}".ToLower();

        var queueClient = queueServiceClient.GetQueueClient(queueName);
        return queueClient.CreateAsync();
    }

    private static TableClientOptions CreateTableNoRetryOptions()
    {
        var retryOptions = new TableClientOptions();
        retryOptions.Retry.MaxRetries = 0;

        return retryOptions;
    }

    private static Task CreateTableAsync(TableServiceClient tableServiceClient)
    {
        var tableName = $"Test{Guid.NewGuid().ToString("N")}".ToLower();

        var tableClient = tableServiceClient.GetTableClient(tableName);
        return tableClient.CreateAsync();
    }
}
