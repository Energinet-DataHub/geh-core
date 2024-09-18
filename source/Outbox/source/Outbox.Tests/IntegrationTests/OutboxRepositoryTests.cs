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
using Energinet.DataHub.Core.Outbox.Tests.IntegrationTests.Fixture;
using Energinet.DataHub.Core.Outbox.Tests.IntegrationTests.Fixture.Database;
using FluentAssertions;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.Outbox.Tests.IntegrationTests;

public class OutboxRepositoryTests : IClassFixture<OutboxFixture>, IAsyncLifetime
{
    private readonly OutboxFixture _fixture;

    public OutboxRepositoryTests(OutboxFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.DatabaseManager.TruncateOutboxTableAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessageIdsAsync_WhenMessageIsUnpublished_ReturnsOutboxMessage()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 17, 13, 37);

        var outboxMessage = new OutboxMessage(now, "type", "data");
        var expectedOutboxMessageId = outboxMessage.Id;

        await using (var arrangeOutboxContext = CreateDbContext())
        {
            arrangeOutboxContext.Add(outboxMessage);
            await arrangeOutboxContext.SaveChangesAsync();
        }

        await using var outboxContext = CreateDbContext();
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
        var publishedAt = Instant.FromUtc(2024, 09, 17, 13, 37);
        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(publishedAt);

        var outboxMessage = new OutboxMessage(publishedAt, "type", "data");
        outboxMessage.SetAsProcessed(clock.Object);

        await using (var arrangeOutboxContext = CreateDbContext())
        {
            arrangeOutboxContext.Add(outboxMessage);
            await arrangeOutboxContext.SaveChangesAsync();
        }

        await using var outboxContext = CreateDbContext();
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
        var processingAt = Instant.FromUtc(2024, 09, 17, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(processingAt);

        var outboxMessage = new OutboxMessage(processingAt, "type", "data");
        outboxMessage.SetAsProcessing(clock.Object);

        var expectedOutboxMessageId = outboxMessage.Id;

        await using (var arrangeOutboxContext = CreateDbContext())
        {
            arrangeOutboxContext.Add(outboxMessage);
            await arrangeOutboxContext.SaveChangesAsync();
        }

        await using var outboxContext = CreateDbContext();
        var outboxRepository = new OutboxRepository(outboxContext, clock.Object);

        // Act

        // => Set clock to before "DurationBetweenProcessingAttempts", so the message should not be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(processingAt.Plus(OutboxMessage.DurationBetweenProcessingAttempts).Minus(Duration.FromSeconds(1)));

        var noExpectedResult = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);
        noExpectedResult.Should().BeEmpty();

        // => Set clock to "DurationBetweenProcessingAttempts", so the message should be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(processingAt.Plus(OutboxMessage.DurationBetweenProcessingAttempts));
        var result = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);

        // Assert
        result.Should().ContainSingle(o => o == expectedOutboxMessageId);
    }

    [Fact]
    public async Task GetUnprocessedOutboxMessageIdsAsync_WhenMessageIsFailed_ReturnsMessageAfterTimeout()
    {
        // Arrange
        var failedAt = Instant.FromUtc(2024, 09, 17, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(failedAt);

        var outboxMessage = new OutboxMessage(failedAt, "type", "data");
        outboxMessage.SetAsFailed(clock.Object, "failed");

        var expectedOutboxMessageId = outboxMessage.Id;

        await using (var arrangeOutboxContext = CreateDbContext())
        {
            arrangeOutboxContext.Add(outboxMessage);
            await arrangeOutboxContext.SaveChangesAsync();
        }

        await using var outboxContext = CreateDbContext();
        var outboxRepository = new OutboxRepository(outboxContext, clock.Object);
        // Act
        // => Set clock to before "MinimumDurationBetweenFailedAttempts", so the message should not be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(failedAt.Plus(OutboxMessage.MinimumDurationBetweenFailedAttempts).Minus(Duration.FromSeconds(1)));

        var noExpectedResult = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);
        noExpectedResult.Should().BeEmpty();

        // => Set clock to "MinimumDurationBetweenFailedAttempts", so the message should be returned
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(failedAt.Plus(OutboxMessage.MinimumDurationBetweenFailedAttempts));
        var result = await outboxRepository.GetUnprocessedOutboxMessageIdsAsync(1000, CancellationToken.None);

        // Assert
        result.Should().ContainSingle(o => o == expectedOutboxMessageId);
    }

    private TestOutboxContext CreateDbContext()
    {
        return _fixture.DatabaseManager.CreateDbContext();
    }
}
