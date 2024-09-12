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
using ExampleHost.WebApi.DbContext;
using ExampleHost.WebApi.UserCreatedEmailOutboxMessage;

namespace ExampleHost.WebApi.UseCases;

public class CreateUserService(IOutboxClient outboxClient, MyApplicationDbContext dbContext)
{
    private readonly IOutboxClient _outboxClient = outboxClient;
    private readonly MyApplicationDbContext _dbContext = dbContext;

    public async Task CreateAsync(string email)
    {
        var userId = CreateUser(email);

        var sendEmailOutboxMessage = new UserCreatedEmailOutboxMessageV1(userId, email);
        await _outboxClient.AddToOutboxAsync(sendEmailOutboxMessage)
            .ConfigureAwait(false);

        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }

    private Guid CreateUser(string email)
    {
        // A real world implementation would create a user in the database through the MyApplicationDbContext
        return Guid.NewGuid();
    }
}
