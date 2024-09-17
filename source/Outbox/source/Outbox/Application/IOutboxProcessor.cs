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

namespace Energinet.DataHub.Core.Outbox.Application;

/// <summary>
/// Outbox processor responsible for publishing outbox messages that hasn't been processed yet.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Processes the outbox, publishing outbox messages that hasn't been processed yet.
    /// </summary>
    /// <param name="limit">The limit of messages to process each time the method is called.</param>
    /// <param name="cancellationToken"></param>
    Task ProcessOutboxAsync(int limit = 1000, CancellationToken? cancellationToken = null);

    /// <summary>
    /// Process outbox message in a new scope, to avoid situations where one message failing stops future messages
    /// from processing. Will not process a message if it is already processed, processing or failed.
    /// <remarks>
    /// Uses CancellationToken until the outbox message has begun publishing, after processing
    /// have begun we want to save the changes before cancelling the task.
    /// </remarks>
    /// </summary>
    Task ProcessOutboxMessageAsync(OutboxMessageId outboxMessageId, CancellationToken cancellationToken);
}
