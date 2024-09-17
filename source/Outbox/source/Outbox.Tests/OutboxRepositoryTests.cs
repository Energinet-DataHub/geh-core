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

using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.Outbox.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.Outbox.Tests;

public class OutboxRepositoryTests
{
    [Fact]
    public async Task GetUnprocessedOutboxMessageIdsAsync_WhenMessageIsUnpublished_ReturnsOutboxMessage()
    {
        // Arrange
        await using var outboxContext = CreateInMemoryDbContext();

        var now = Instant.FromUtc(2024, 09, 17, 13, 37);

        var outboxMessage = new OutboxMessage(now, "type", "data");
        var expectedOutboxMessageId = outboxMessage.Id;

        outboxContext.Add(outboxMessage);
        await outboxContext.SaveChangesAsync();

        var outboxRepository = new OutboxRepository(outboxContext, Mock.Of<IClock>());

        // Act
        var result = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);

        // Assert
        result.Should().ContainSingle(o => o == expectedOutboxMessageId);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessageIdsAsync_WhenMessageIsPublished_DoesNotReturnMessage()
    {
        // Arrange
        await using var outboxContext = CreateInMemoryDbContext();

        var publishedAt = Instant.FromUtc(2024, 09, 17, 13, 37);
        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(publishedAt);

        var outboxMessage = new OutboxMessage(publishedAt, "type", "data");
        outboxMessage.SetAsProcessed(clock.Object);

        outboxContext.Add(outboxMessage);
        await outboxContext.SaveChangesAsync();

        var outboxRepository = new OutboxRepository(outboxContext, clock.Object);

        // Act
        var result = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessageIdsAsync_WhenMessageIsProcessing_ReturnsMessageAfterTimeout()
    {
        // Arrange
        await using var outboxContext = CreateInMemoryDbContext();

        var processingAt = Instant.FromUtc(2024, 09, 17, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(processingAt);

        var outboxMessage = new OutboxMessage(processingAt, "type", "data");
        outboxMessage.SetAsProcessing(clock.Object);

        var expectedOutboxMessageId = outboxMessage.Id;

        outboxContext.Add(outboxMessage);
        await outboxContext.SaveChangesAsync();

        var outboxRepository = new OutboxRepository(outboxContext, clock.Object);

        // Act

        // => Set clock to before "ProcessingTimeout", so the message should not be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(processingAt.Plus(OutboxMessage.ProcessingTimeout).Minus(Duration.FromSeconds(1)));

        var noExpectedResult = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);
        noExpectedResult.Should().BeEmpty();

        // => Set clock to "ProcessingTimeout", so the message should be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(processingAt.Plus(OutboxMessage.ProcessingTimeout));
        var result = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);

        // Assert
        result.Should().ContainSingle(o => o == expectedOutboxMessageId);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessageIdsAsync_WhenMessageIsFailed_ReturnsMessageAfterTimeout()
    {
        // Arrange
        await using var outboxContext = CreateInMemoryDbContext();

        var failedAt = Instant.FromUtc(2024, 09, 17, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(failedAt);

        var outboxMessage = new OutboxMessage(failedAt, "type", "data");
        outboxMessage.SetAsFailed(clock.Object, "failed");

        var expectedOutboxMessageId = outboxMessage.Id;

        outboxContext.Add(outboxMessage);
        await outboxContext.SaveChangesAsync();

        var outboxRepository = new OutboxRepository(outboxContext, clock.Object);

        // Act

        // => Set clock to before "MinimumErrorRetryTimeout", so the message should not be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(failedAt.Plus(OutboxMessage.MinimumErrorRetryTimeout).Minus(Duration.FromSeconds(1)));

        var noExpectedResult = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);
        noExpectedResult.Should().BeEmpty();

        // => Set clock to "MinimumErrorRetryTimeout", so the message should be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(failedAt.Plus(OutboxMessage.MinimumErrorRetryTimeout));
        var result = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);

        // Assert
        result.Should().ContainSingle(o => o == expectedOutboxMessageId);
    }

    private static TestOutboxContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TestOutboxContext>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase-{Guid.NewGuid()}")
            .Options;
        return new TestOutboxContext(options);
    }
}
