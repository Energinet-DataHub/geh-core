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
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.Azurite
{
    public class AzuriteManagerTests
    {
        [Collection(nameof(AzuriteCollectionFixture))]
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

        [Collection(nameof(AzuriteCollectionFixture))]
        public sealed class VerifyAzuriteCanBeUsedWithOAuth : IDisposable
        {
            public VerifyAzuriteCanBeUsedWithOAuth(ITestOutputHelper testOutputHelper)
            {
                AzuriteManager = new AzuriteManager();
                AzuriteManager.StartAzurite(useOAuth: true);

                NoRetryOptions = new BlobClientOptions();
                NoRetryOptions.Retry.MaxRetries = 0;

                TestLogger = new TestDiagnosticsLogger
                {
                    TestOutputHelper = testOutputHelper,
                };
            }

            private AzuriteManager AzuriteManager { get; }

            private BlobClientOptions NoRetryOptions { get; }

            private TestDiagnosticsLogger TestLogger { get; }

            public void Dispose()
            {
                AzuriteManager.Dispose();
            }

            [Fact]
            public async Task When_UseDevelopmentStorageShortcut_Then_CreateContainerShouldFail()
            {
                // Arrange
                var client = new BlobServiceClient(
                    connectionString: "UseDevelopmentStorage=true",
                    NoRetryOptions);

                // Act
                var exception = await Record.ExceptionAsync(() => CreateStorageContainerAsync(client));

                // Assert
                exception.Should().NotBeNull();
            }

            /// <summary>
            /// When using Azurite with OAuth we must use Https and 'localhost' (not '127.0.0.1').
            /// </summary>
            [Fact]
            public async Task When_UsingConnectionString_Then_CanCreateContainer()
            {
                // Arrange
                // Uses the well-known storage account name and key.
                // See: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&bc=%2Fazure%2Fstorage%2Fblobs%2Fbreadcrumb%2Ftoc.json&tabs=visual-studio#well-known-storage-account-and-key
                var wellKnownStorageAccountName = "devstoreaccount1";
                var wellKnownStorageAccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

                // TODO: Expose property on AzuriteManager for "blob connection string"
                // TODO: Expose property on AzuriteManager for "blob uri"

                var client = new BlobServiceClient(
                    connectionString: $"DefaultEndpointsProtocol=https;AccountName={wellKnownStorageAccountName};AccountKey={wellKnownStorageAccountKey};BlobEndpoint=https://localhost:10000/devstoreaccount1;",
                    NoRetryOptions);

                // Act
                var exception = await Record.ExceptionAsync(() => CreateStorageContainerAsync(client));

                // Assert
                if (exception != null)
                {
                    TestLogger.WriteLine($"FULL EXCEPTION: {exception.ToString()}");
                }

                exception.Should().BeNull();
            }

            [Fact]
            public async Task When_UsingHttpsAndTokenCredential_Then_CanCreateContainer()
            {
                // Arrange
                var client = new BlobServiceClient(
                    serviceUri: new Uri("https://localhost:10000/devstoreaccount1"),
                    credential: new DefaultAzureCredential(),
                    NoRetryOptions);

                // Act
                var exception = await Record.ExceptionAsync(() => CreateStorageContainerAsync(client));

                // Assert
                if (exception != null)
                {
                    TestLogger.WriteLine($"FULL EXCEPTION: {exception.ToString()}");
                }

                exception.Should().BeNull();
            }

            private static Task CreateStorageContainerAsync(BlobServiceClient blobServiceClient)
            {
                var containerName = $"Test{Guid.NewGuid()}".ToLower();

                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                return blobContainerClient.CreateAsync();
            }
        }
    }
}
