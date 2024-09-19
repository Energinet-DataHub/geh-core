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

using Azure.Identity;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;

namespace Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Fixtures;

public class BlobFixture : IAsyncLifetime
{
    public BlobFixture()
    {
        AzuriteManager = new AzuriteManager(useOAuth: true);

        BlobContainerName = Guid.NewGuid().ToString().ToLower();
        BlobServiceClient = new BlobServiceClient(AzuriteManager.BlobStorageServiceUri, new DefaultAzureCredential());
    }

    public AzuriteManager AzuriteManager { get; }

    public string BlobContainerName { get; }

    public BlobServiceClient BlobServiceClient { get; }

    public Task InitializeAsync()
    {
        AzuriteManager.StartAzurite();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        AzuriteManager.Dispose();

        return Task.CompletedTask;
    }
}
