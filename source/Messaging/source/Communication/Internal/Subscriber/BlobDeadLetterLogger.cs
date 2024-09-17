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

using System.Text;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

/// <inheritdoc cref="IDeadLetterLogger"/>
internal class BlobDeadLetterLogger : IDeadLetterLogger
{
    private readonly ILogger _logger;
    private readonly BlobDeadLetterLoggerOptions _options;
    private readonly BlobServiceClient _client;

    public BlobDeadLetterLogger(
        ILogger<BlobDeadLetterLogger> logger,
        IOptions<BlobDeadLetterLoggerOptions> blobOptions,
        IAzureClientFactory<BlobServiceClient> clientFactory)
    {
        _logger = logger;
        _options = blobOptions.Value;
        _client = clientFactory.CreateClient(_options.ContainerName);
    }

    public async Task LogAsync(string deadLetterSource, ServiceBusReceivedMessage message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deadLetterSource);
        ArgumentNullException.ThrowIfNull(message);

        var containerClient = _client.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

        try
        {
            var blobName = BuildBlobName(deadLetterSource, message);
            var blobClient = containerClient.GetBlobClient(blobName);

            await CreateBlobAsync(blobClient, message).ConfigureAwait(false);
            await AddMetadataToBlobAsync(blobClient, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log dead-letter message. MessageId: {MessageId}", message.MessageId);

            throw;
        }
    }

    private static async Task CreateBlobAsync(BlobClient blobClient, ServiceBusReceivedMessage message)
    {
        var bodyAsBase64 = Convert.ToBase64String(message.Body);
        await blobClient.UploadAsync(BinaryData.FromString(bodyAsBase64), overwrite: true).ConfigureAwait(false);
    }

    // TODO: Should we remove this again; e.g. if we save values into the blob name OR should we Url encode the values and implement a backup
    private static async Task AddMetadataToBlobAsync(BlobClient blobClient, ServiceBusReceivedMessage message)
    {
        var metadata = new Dictionary<string, string>
        {
            ["MessageId"] = message.MessageId ?? string.Empty,
            ["MessageSubject"] = message.Subject ?? string.Empty,
        };
        await blobClient.SetMetadataAsync(metadata).ConfigureAwait(false);
    }

    private static string BuildBlobName(string deadLetterSource, ServiceBusReceivedMessage message)
    {
        var sb = new StringBuilder();

        sb.Append(deadLetterSource);
        sb.Append('_');
        sb.Append(message.MessageId ?? "message-id-null");
        sb.Append('_');
        sb.Append(message.Subject ?? "subject-null");

        return sb.ToString();
    }
}
