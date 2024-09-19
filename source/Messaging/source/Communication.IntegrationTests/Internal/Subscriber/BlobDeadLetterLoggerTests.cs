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
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Messaging.Communication.Extensions.Options;
using Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Fixtures;
using Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using FluentAssertions;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Internal.Subscriber;

public class BlobDeadLetterLoggerTests : IClassFixture<BlobDeadLetterLoggerFixture>, IAsyncLifetime
{
    public BlobDeadLetterLoggerTests(BlobDeadLetterLoggerFixture fixture)
    {
        Fixture = fixture;
    }

    private BlobDeadLetterLoggerFixture Fixture { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // Clear data after each test
        Fixture.BlobServiceClient
            .GetBlobContainerClient(Fixture.BlobContainerName)
            .DeleteIfExists();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task LogAsync_WhenContainerDoesNotExist_ContainerIsCreatedAndMessageSaved()
    {
        // Arrange
        var sut = Fixture.ServiceProvider.GetRequiredService<IDeadLetterLogger>();

        var deadLetterSource = "inbox-events";
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            messageId: Guid.NewGuid().ToString(),
            subject: "event-name",
            body: new BinaryData("content"));

        // Act
        await sut.LogAsync(deadLetterSource, message);

        // Assert
        // => Verify blob was created as expected; name is a combination of 'deadLetterSource', 'MessageId' and 'Subject'
        var blobName = $"{deadLetterSource}_{message.MessageId}_{message.Subject}";
        var blobClient = Fixture.BlobServiceClient
            .GetBlobContainerClient(Fixture.BlobContainerName)
            .GetBlobClient(blobName);
        blobClient.Exists().Value.Should().BeTrue();
    }
}
