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
using Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Fixtures;
using Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Internal.Subscriber;

public class BlobDeadLetterLoggerTests : IClassFixture<BlobDeadLetterLoggerFixture>
{
    public BlobDeadLetterLoggerTests(BlobDeadLetterLoggerFixture fixture)
    {
        Fixture = fixture;
    }

    private BlobDeadLetterLoggerFixture Fixture { get; }

    [Fact]
    public async Task ContainerDoesNotExist_WhenLogAsync_ContainerIsCreated()
    {
        // Arrange
        // => Explicit show container doesn't exist
        await Fixture.BlobServiceClient
            .GetBlobContainerClient(Fixture.BlobContainerName)
            .DeleteIfExistsAsync();

        var sut = Fixture.ServiceProvider.GetRequiredService<IDeadLetterLogger>();

        var deadLetterSource = "inbox-events";
        var message = CreateMessage();

        // Act
        await sut.LogAsync(deadLetterSource, message);

        // Assert
        Fixture.BlobServiceClient
            .GetBlobContainerClient(Fixture.BlobContainerName)
            .Exists().Value.Should().BeTrue();
    }

    [Fact]
    public async Task ContainerMightExist_WhenLogAsync_MessageIsSaved()
    {
        // Arrange
        var sut = Fixture.ServiceProvider.GetRequiredService<IDeadLetterLogger>();

        var deadLetterSource = "inbox-events";
        var message = CreateMessage();

        // Act
        await sut.LogAsync(deadLetterSource, message);

        // Assert
        // => Verify blob was created as expected; name is a combination of 'deadLetterSource', 'MessageId' and 'Subject'
        var blobName = $"{deadLetterSource}_{message.MessageId}_{message.Subject}";
        var blobClient = Fixture.BlobServiceClient
            .GetBlobContainerClient(Fixture.BlobContainerName)
            .GetBlobClient(blobName);
        blobClient.Exists().Value.Should().BeTrue();

        var content = await blobClient.DownloadContentAsync();
        content.Value.Content.ToString().Should().Be(Convert.ToBase64String(message.Body));
    }

    private static ServiceBusReceivedMessage CreateMessage()
    {
        return ServiceBusModelFactory.ServiceBusReceivedMessage(
            messageId: Guid.NewGuid().ToString(),
            subject: "event-name",
            body: new BinaryData("content"));
    }
}
