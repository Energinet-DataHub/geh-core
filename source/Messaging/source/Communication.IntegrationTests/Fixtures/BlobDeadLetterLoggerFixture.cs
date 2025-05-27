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

using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Fixtures;

public class BlobDeadLetterLoggerFixture : IAsyncLifetime
{
    public BlobDeadLetterLoggerFixture()
    {
        AzuriteManager = new AzuriteManager(useOAuth: true);

        BlobContainerName = Guid.NewGuid().ToString().ToLower();
        IntegrationTestConfiguration = new IntegrationTestConfiguration();
        BlobServiceClient = new BlobServiceClient(AzuriteManager.BlobStorageServiceUri, IntegrationTestConfiguration.Credential);

        ServiceProvider = BuildServiceProvider(AzuriteManager.BlobStorageServiceUri.OriginalString, BlobContainerName);
    }

    public AzuriteManager AzuriteManager { get; }

    public string BlobContainerName { get; }

    public BlobServiceClient BlobServiceClient { get; }

    public ServiceProvider ServiceProvider { get; }

    private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

    public Task InitializeAsync()
    {
        AzuriteManager.StartAzurite();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        AzuriteManager.Dispose();
        await ServiceProvider.DisposeAsync();
    }

    private ServiceProvider BuildServiceProvider(string storageAccountUrl, string blobContainerName)
    {
        var services = new ServiceCollection();

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                [$"{BlobDeadLetterLoggerOptions.SectionName}:{nameof(BlobDeadLetterLoggerOptions.StorageAccountUrl)}"] = storageAccountUrl,
                [$"{BlobDeadLetterLoggerOptions.SectionName}:{nameof(BlobDeadLetterLoggerOptions.ContainerName)}"] = blobContainerName,
            })
            .Build();

        services
            .AddScoped<IConfiguration>(_ => configurationRoot)
            .AddLogging()
            .AddDeadLetterHandlerForIsolatedWorker(
                configurationRoot,
                _ => IntegrationTestConfiguration.Credential);

        return services.BuildServiceProvider();
    }
}
