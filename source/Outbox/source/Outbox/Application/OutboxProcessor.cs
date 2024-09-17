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
using Energinet.DataHub.Core.Outbox.Infrastructure.Dependencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Core.Outbox.Application;

public class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxScopeFactory _outboxScopeFactory;
    private readonly IClock _clock;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IOutboxScopeFactory outboxScopeFactory,
        IClock clock,
        ILogger<OutboxProcessor> logger)
    {
        _clock = clock ?? throw new NullReferenceException(
            "IClock is required when using the IOutboxProcessor. " +
            "Has NodaTime been added to the dependency injection container?");

        _logger = logger ?? throw new NullReferenceException(
            "ILogger is required when using the IOutboxProcessor. " +
            "Has ILogger been added to the dependency injection container?");

        _outboxScopeFactory = outboxScopeFactory;
    }

    /// <inheritdoc />
    public async Task ProcessOutboxAsync(int limit = 1000, CancellationToken? cancellationToken = null)
    {
        cancellationToken ??= CancellationToken.None;

        using var outerScope = _outboxScopeFactory.CreateScopedDependencies();

        var outboxMessageIds = await outerScope.OutboxRepository
            .GetUnprocessedOutboxMessageIdsAsync(limit, cancellationToken.Value)
            .ConfigureAwait(false);

        if (outboxMessageIds.Count > 0)
            _logger.LogInformation("Processing {OutboxMessageCount} outbox messages", outboxMessageIds.Count);

        foreach (var outboxMessageId in outboxMessageIds)
        {
            cancellationToken.Value.ThrowIfCancellationRequested();

            try
            {
                await ProcessOutboxMessageAsync(outboxMessageId, cancellationToken.Value)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await SetAsFailedAsync(outboxMessageId, ex)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async Task ProcessOutboxMessageAsync(OutboxMessageId outboxMessageId, CancellationToken cancellationToken)
    {
        using var innerScope = _outboxScopeFactory.CreateScopedDependencies();
        var outboxContext = innerScope.OutboxContext;
        var outboxRepository = innerScope.OutboxRepository;
        var outboxPublishers = innerScope.OutboxPublishers;

        var outboxMessage = await outboxRepository.GetAsync(outboxMessageId, cancellationToken)
            .ConfigureAwait(false);

        if (!outboxMessage.ShouldProcessNow(_clock))
            return;

        try
        {
            outboxMessage.SetAsProcessing(_clock);
            await outboxContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Outbox message with id {OutboxMessageId} processing was already started",
                outboxMessageId);
            return;
        }

        // Process outbox message
        var outboxMessagePublisher = outboxPublishers
            .SingleOrDefault(p => p.CanPublish(outboxMessage.Type));

        if (outboxMessagePublisher == null)
            throw new InvalidOperationException($"No processor found for outbox message type {outboxMessage.Type} and id {outboxMessage.Id}");

        await outboxMessagePublisher.PublishAsync(outboxMessage.Payload)
            .ConfigureAwait(false);

        outboxMessage.SetAsProcessed(_clock);

        await outboxContext
            // ReSharper disable once MethodSupportsCancellation
            // We want to save the changes before cancelling the task, since the outbox message is already published
            .SaveChangesAsync(CancellationToken.None)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Set as failed in a new scope, to avoid situations where the exception is cached on the existing db context.
    /// <remarks>Uses CancellationToken.None since we want to save the error even if cancellation is requested</remarks>
    /// </summary>
    private async Task SetAsFailedAsync(OutboxMessageId outboxMessageId, Exception exception)
    {
        _logger.LogError(
            exception,
            "Failed to process outbox message with id {OutboxMessageId}",
            outboxMessageId);
        using var errorScope = _outboxScopeFactory.CreateScopedDependencies();
        var outboxContext = errorScope.OutboxContext;
        var outboxRepository = errorScope.OutboxRepository;

        var outgoingMessage = await outboxRepository.GetAsync(outboxMessageId, CancellationToken.None)
            .ConfigureAwait(false);

        outgoingMessage.SetAsFailed(_clock, exception.ToString());

        await outboxContext.SaveChangesAsync(CancellationToken.None)
            .ConfigureAwait(false);
    }
}
