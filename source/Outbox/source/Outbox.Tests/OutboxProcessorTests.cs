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

using Energinet.DataHub.Core.Outbox.Abstractions;
using Energinet.DataHub.Core.Outbox.Application;
using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.Outbox.Infrastructure;
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Energinet.DataHub.Core.Outbox.Infrastructure.Dependencies;
using Energinet.DataHub.Core.Outbox.Tests.Fixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.Outbox.Tests;

public class OutboxProcessorTests
{
    [Fact]
    public async Task GivenOutboxMessage_WhenOutboxIsProcessed_ThenOutboxMessageIsPublished()
    {
        // Arrange
        var now = Instant.FromUtc(2024, 09, 11, 13, 37);
        var inThePast = now.Minus(Duration.FromHours(1));

        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetCurrentInstant())
            .Returns(now);

        var logger = new Mock<ILogger<OutboxProcessor>>();
        var outboxScopeFactory = new Mock<IOutboxScopeFactory>();

        var expectedType = "mock-type";
        var expectedPayload = "mock-payload";
        var outboxMessage = new OutboxMessage(
            createdAt: inThePast,
            type: expectedType,
            payload: expectedPayload);

        var outboxRepository = new Mock<IOutboxRepository>();
        outboxRepository
            .Setup(r => r.GetUnprocessedOutboxMessageIdsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([outboxMessage.Id]);
        outboxRepository
            .Setup(r => r.GetAsync(
                It.Is<OutboxMessageId>(id => id == outboxMessage.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(outboxMessage);

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

        var outboxProcessor = new OutboxProcessor(outboxScopeFactory.Object, clock.Object, logger.Object);

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
}
