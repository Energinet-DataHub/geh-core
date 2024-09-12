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
using Energinet.DataHub.Core.Outbox.Domain;
using Energinet.DataHub.Core.Outbox.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.Core.Outbox.Infrastructure;

internal class OutboxRepository(IOutboxContext outboxContext) : IOutboxRepository
{
    private readonly IOutboxContext _outboxContext = outboxContext;

    public void Add(OutboxMessage outboxMessage)
    {
        _outboxContext.Outbox.Add(outboxMessage);
    }

    public async Task<IReadOnlyCollection<OutboxMessageId>> GetUnprocessedOutboxMessageIdsAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var outboxMessageIds = await _outboxContext.Outbox
            .Where(om => om.PublishedAt == null)
            .OrderBy(om => om.CreatedAt)
            .Select(om => om.Id)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return outboxMessageIds;
    }

    public Task<OutboxMessage> GetAsync(OutboxMessageId outboxMessageId, CancellationToken cancellationToken)
    {
        return _outboxContext.Outbox.SingleAsync(om => om.Id == outboxMessageId, cancellationToken);
    }
}
