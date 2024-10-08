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

using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.Core.Outbox.Application;
using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Energinet.DataHub.Core.Outbox.Infrastructure.Dependencies;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.Outbox.Tests.UnitTests;

public class OutboxProcessorTests
{
    [Fact]
    public async Task GivenOutboxMessage_WhenOutboxIsProcessed_ThenOutboxMessageIsPublished()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 11, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(now);

        var expectedType = "mock-type";
        var expectedPayload = "mock-payload";
        var outboxMessage = new OutboxMessage(
            createdAt: now,
            type: expectedType,
            payload: expectedPayload);

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            [outboxMessage],
            expectedType,
            expectedPayload);

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            Mock.Of<ILogger<OutboxProcessor>>());

        // Act
        await outboxProcessor.ProcessOutboxAsync();

        // Assert
        outboxPublisher.Verify(p => p.CanPublish(expectedType), Times.Once);
        outboxPublisher.Verify(p => p.PublishAsync(expectedPayload), Times.Once);
        outboxPublisher.VerifyNoOtherCalls();

        using var scope = new AssertionScope();
        outboxMessage.PublishedAt.Should().Be(now);
        outboxMessage.ErrorCount.Should().Be(0);
        outboxMessage.FailedAt.Should().Be(null);
    }

    [Fact]
    public async Task GivenMultipleOutboxMessages_WhenOutboxIsProcessed_ThenEachMessageIsPublishedInSeparateScopes()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 11, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(now);

        var expectedMessagesCount = 22;

        var expectedType = "mock-type";
        var expectedPayload = "mock-payload";
        var outboxMessages = Enumerable.Range(0, expectedMessagesCount)
            .Select(_ => new OutboxMessage(now, expectedType, expectedPayload))
            .ToList();

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            outboxMessages,
            expectedType,
            expectedPayload);

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            Mock.Of<ILogger<OutboxProcessor>>());

        // Act
        await outboxProcessor.ProcessOutboxAsync();

        // Assert
        outboxPublisher.Verify(p => p.CanPublish(expectedType), Times.Exactly(expectedMessagesCount));
        outboxPublisher.Verify(p => p.PublishAsync(expectedPayload), Times.Exactly(expectedMessagesCount));
        outboxPublisher.VerifyNoOtherCalls();

        using var scope = new AssertionScope();
        outboxMessages.Should().AllSatisfy(om => om.PublishedAt.Should().Be(now));
        outboxScopeFactory.Verify(
            f => f.CreateScopedDependencies(),
            Times.Exactly(expectedMessagesCount + 1)); // expectedMessagesCount + 1 for the outer scope
        // outboxScopeFactory.VerifyNoOtherCalls(); // TODO: Why does this verify calls to child elements as well?
    }

    [Fact]
    public async Task GivenOutboxMessage_WhenProcessingFails_ThenOutboxMessageIsMarkedAsFailed()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 11, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(now);

        var expectedType = "mock-type";
        var expectedPayload = "mock-payload";
        var outboxMessage = new OutboxMessage(
            createdAt: now,
            type: expectedType,
            payload: expectedPayload);

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            [outboxMessage],
            expectedType,
            expectedPayload);

        outboxPublisher.Setup(p => p.PublishAsync(expectedPayload))
            .ThrowsAsync(new Exception("Publishing failed"));

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            Mock.Of<ILogger<OutboxProcessor>>());

        // Act
        await outboxProcessor.ProcessOutboxAsync();

        // Assert
        outboxPublisher.Verify(p => p.CanPublish(expectedType), Times.Once);
        outboxPublisher.Verify(p => p.PublishAsync(expectedPayload), Times.Once);
        outboxPublisher.VerifyNoOtherCalls();

        using var scope = new AssertionScope();
        outboxMessage.PublishedAt.Should().BeNull();
        outboxMessage.ErrorCount.Should().Be(1);
        outboxMessage.FailedAt.Should().Be(now);
    }

    [Fact]
    public async Task GivenFailedOutboxMessage_WhenOutboxIsProcessed_ThenOutboxMessageIsRetriedAfterTimeout()
    {
        // Arrange
        var failedAt = Instant.FromUtc(2024, 09, 11, 13, 37);
        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant()).Returns(failedAt);

        var expectedType = "mock-type";
        var expectedPayload = "mock-payload";
        var outboxMessage = new OutboxMessage(
            createdAt: failedAt,
            type: expectedType,
            payload: expectedPayload);
        outboxMessage.SetAsFailed(clock.Object, "Initial failure");

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            [outboxMessage],
            expectedType,
            expectedPayload);

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            Mock.Of<ILogger<OutboxProcessor>>());

        // => Setup clock to be before "MinimumDurationBetweenFailedAttempts", so it should not be retried
        var beforeRetryTimeout = failedAt
            .Plus(OutboxMessage.MinimumDurationBetweenFailedAttempts)
            .Minus(Duration.FromSeconds(1));
        clock.Setup(c => c.GetCurrentInstant()).Returns(beforeRetryTimeout);

        // => Immediately re-processing should not retry the failed message
        await outboxProcessor.ProcessOutboxAsync();
        using (new AssertionScope())
        {
            outboxMessage.PublishedAt.Should().BeNull();
            outboxMessage.ErrorCount.Should().Be(1); // Error count should not increase on successful retry
            outboxMessage.FailedAt.Should().Be(failedAt);
        }

        // => Setup clock to be "MinimumDurationBetweenFailedAttempts" after the failed message, so it should be retried
        var afterRetryTimeout = failedAt.Plus(OutboxMessage.MinimumDurationBetweenFailedAttempts);
        clock.Setup(c => c.GetCurrentInstant()).Returns(afterRetryTimeout);

        await outboxProcessor.ProcessOutboxAsync();

        outboxPublisher.Verify(p => p.CanPublish(expectedType), Times.Once);
        outboxPublisher.Verify(p => p.PublishAsync(expectedPayload), Times.Once);
        outboxPublisher.VerifyNoOtherCalls();

        using var scope = new AssertionScope();
        outboxMessage.PublishedAt.Should().Be(afterRetryTimeout);
        outboxMessage.ErrorCount.Should().Be(1); // Error count should not increase on successful retry
        outboxMessage.FailedAt.Should().Be(failedAt);
    }

    [Fact]
    public async Task GivenProcessingOutboxMessage_WhenOutboxIsProcessed_ThenOutboxMessageIsRetriedAfterTimeout()
    {
        // Arrange
        var processingAt = Instant.FromUtc(2024, 09, 11, 13, 37);
        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant()).Returns(processingAt);

        var expectedType = "mock-type";
        var expectedPayload = "mock-payload";
        var outboxMessage = new OutboxMessage(
            createdAt: processingAt,
            type: expectedType,
            payload: expectedPayload);
        outboxMessage.SetAsProcessing(clock.Object);

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            [outboxMessage],
            expectedType,
            expectedPayload);

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            Mock.Of<ILogger<OutboxProcessor>>());

        // => Setup clock to be before "DurationBetweenProcessingAttempts", so it should not be retried
        var afterRetryTimeout = processingAt
            .Plus(OutboxMessage.DurationBetweenProcessingAttempts)
            .Minus(Duration.FromSeconds(1));
        clock.Setup(c => c.GetCurrentInstant()).Returns(afterRetryTimeout);

        await outboxProcessor.ProcessOutboxAsync();
        using (new AssertionScope())
        {
            outboxMessage.PublishedAt.Should().BeNull();
            outboxMessage.ProcessingAt.Should().Be(processingAt);
            outboxMessage.ErrorCount.Should().Be(0);
        }

        // => Setup clock to be "DurationBetweenProcessingAttempts" after the processing message, so it should be retried
        var now = processingAt.Plus(OutboxMessage.DurationBetweenProcessingAttempts);
        clock.Setup(c => c.GetCurrentInstant()).Returns(now);

        // => This time re-processing the outbox should cause the message to be retried
        await outboxProcessor.ProcessOutboxAsync();

        outboxPublisher.Verify(p => p.CanPublish(expectedType), Times.Once);
        outboxPublisher.Verify(p => p.PublishAsync(expectedPayload), Times.Once);
        outboxPublisher.VerifyNoOtherCalls();

        using var scope = new AssertionScope();
        outboxMessage.PublishedAt.Should().Be(now);
        outboxMessage.ProcessingAt.Should().Be(now);
        outboxMessage.ErrorCount.Should().Be(0);
    }

    [Fact]
    public async Task GivenInvalidOutboxMessageType_WhenOutboxIsProcessed_ThenExceptionIsThrownAndMessageIsMarkedAsFailed()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 11, 13, 37);
        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant()).Returns(now);

        var logger = new Mock<ILogger<OutboxProcessor>>();

        var invalidType = "invalid-type";
        var expectedPayload = "mock-payload";
        var outboxMessage = new OutboxMessage(
            createdAt: now,
            type: invalidType,
            payload: expectedPayload);

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            [outboxMessage],
            "expected-type",
            expectedPayload);

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            logger.Object);

        // Act
        await outboxProcessor.ProcessOutboxAsync();

        // Assert
        using var scope = new AssertionScope();
        outboxMessage.PublishedAt.Should().BeNull();
        outboxMessage.ErrorCount.Should().Be(1);
        outboxMessage.FailedAt.Should().Be(now);

        // => Verify an exception is logged containing the outbox message id and the invalid type
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<InvalidOperationException>(ex =>
                    ex.Message.Contains(outboxMessage.Id.Id.ToString())
                    && ex.Message.Contains(invalidType)),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // => Verify the outbox publisher CanPublish method was called with the invalid type
        outboxPublisher.Verify(p => p.CanPublish(invalidType), Times.Once);

        // => Verify the outbox publisher PublishAsync method was not called, since the type is invalid
        outboxPublisher.Verify(p => p.PublishAsync(It.IsAny<string>()), Times.Never);
        outboxPublisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GivenNoOutboxMessages_WhenOutboxIsProcessed_ThenNoMessagesArePublished()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 11, 13, 37);

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(now);

        var (outboxScopeFactory, outboxPublisher) = MockOutboxDependencies(
            [],
            "mock-type",
            "mock-payload");

        var outboxProcessor = new OutboxProcessor(
            outboxScopeFactory.Object,
            clock.Object,
            Mock.Of<ILogger<OutboxProcessor>>());

        // Act
        await outboxProcessor.ProcessOutboxAsync();

        // Assert
        outboxPublisher.Verify(p => p.CanPublish(It.IsAny<string>()), Times.Never);
        outboxPublisher.Verify(p => p.PublishAsync(It.IsAny<string>()), Times.Never);
        outboxPublisher.VerifyNoOtherCalls();
    }

    private static (Mock<IOutboxScopeFactory> ScopeFactory, Mock<IOutboxPublisher> Publisher) MockOutboxDependencies(
        List<OutboxMessage> outboxMessages,
        string expectedType,
        string expectedPayload)
    {
        var outboxScopeFactory = new Mock<IOutboxScopeFactory>();

        var outboxMessageIds = outboxMessages.Select(om => om.Id).ToList();

        var outboxRepository = new Mock<IOutboxRepository>();
        outboxRepository
            .Setup(r => r.GetUnprocessedOutboxMessageIdsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(outboxMessageIds);
        outboxRepository
            .Setup(r => r.GetAsync(
                It.Is<OutboxMessageId>(id => outboxMessageIds.Contains(id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OutboxMessageId id, CancellationToken _) => outboxMessages.Single(om => om.Id == id));

        var outboxPublisher = new Mock<IOutboxPublisher>();
        outboxPublisher.Setup(p => p.CanPublish(expectedType))
            .Returns(true);
        outboxPublisher.Setup(p => p.PublishAsync(expectedPayload))
            .Returns(Task.CompletedTask);

        var scopedOutboxDependencies = new Mock<IScopedOutboxDependencies>();
        scopedOutboxDependencies.SetupGet(o => o.OutboxContext)
            .Returns(Mock.Of<IOutboxContext>());
        scopedOutboxDependencies
            .SetupGet(o => o.OutboxRepository)
            .Returns(outboxRepository.Object);
        scopedOutboxDependencies
            .SetupGet(o => o.OutboxPublishers)
            .Returns([outboxPublisher.Object]);

        outboxScopeFactory
            .Setup(o => o.CreateScopedDependencies())
            .Returns(scopedOutboxDependencies.Object);

        return (outboxScopeFactory, outboxPublisher);
    }
}
