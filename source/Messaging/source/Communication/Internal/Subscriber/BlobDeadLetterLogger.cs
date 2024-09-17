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

using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

/// <inheritdoc cref="IDeadLetterLogger"/>
internal class BlobDeadLetterLogger : IDeadLetterLogger
{
    private readonly BlobDeadLetterLoggerOptions _options;
    private readonly BlobServiceClient _client;

    public BlobDeadLetterLogger(
        IOptions<BlobDeadLetterLoggerOptions> blobOptions,
        IAzureClientFactory<BlobServiceClient> clientFactory)
    {
        _options = blobOptions.Value;
        _client = clientFactory.CreateClient(_options.ContainerName);
    }

    public async Task LogAsync(ServiceBusReceivedMessage message)
    {
        var containerClient = _client.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

        // KISS: Use a unique blob name to avoid handling all kind of error scenarious (e.g. invalid MessageId, name exists etc.)
        var blobName = Guid.NewGuid().ToString();
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(message.Body, overwrite: true).ConfigureAwait(false);
    }
}
