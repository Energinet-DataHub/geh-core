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
using Energinet.DataHub.Core.Outbox.Domain;
using NodaTime;

namespace Energinet.DataHub.Core.Outbox.Application;

public class OutboxClient(
    IClock clock,
    IOutboxRepository outboxRepository)
    : IOutboxClient
{
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(
        nameof(clock),
        "IClock is required when using the IOutboxClient. Has NodaTime been added to the dependency injection container?");

    private readonly IOutboxRepository _outboxRepository = outboxRepository;

    /// <inheritdoc />
    public async Task AddToOutboxAsync<T>(IOutboxMessage<T> message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var payload = await message.SerializeAsync()
            .ConfigureAwait(false);

        var outboxMessage = new OutboxMessage(
            _clock.GetCurrentInstant(),
            message.Type,
            payload);

        _outboxRepository.Add(outboxMessage);
    }
}
