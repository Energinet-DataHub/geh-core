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

using System.Text.Json;
using Energinet.DataHub.Core.Outbox.Abstractions;

namespace ExampleHost.WebApi.UserCreatedEmailOutboxMessage;

public class UserCreatedEmailOutboxMessagePublisher : IOutboxPublisher
{
    public bool CanPublish(string type) => type.Equals(UserCreatedEmailOutboxMessageV1.OutboxMessageType);

    public Task PublishAsync(string serializedPayload)
    {
        var payload = JsonSerializer.Deserialize<UserCreatedEmailOutboxMessageV1Payload>(serializedPayload)
                      ?? throw new InvalidOperationException($"Failed to deserialize payload of type {nameof(UserCreatedEmailOutboxMessageV1Payload)}");

        Console.WriteLine($"Payload id={payload.Id}, email={payload.Email}");

        // Implementation of publishing the message, e.g. sending an email, sending a http request, adding to a service bus etc.
        return Task.CompletedTask;
    }
}
