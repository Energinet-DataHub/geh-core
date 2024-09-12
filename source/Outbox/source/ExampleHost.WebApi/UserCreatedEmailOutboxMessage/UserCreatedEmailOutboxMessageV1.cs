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

public record UserCreatedEmailOutboxMessageV1
    : IOutboxMessage<UserCreatedEmailOutboxMessageV1Payload>
{
    public const string OutboxMessageType = "UserCreatedEmailOutboxMessageV1";

    public UserCreatedEmailOutboxMessageV1(Guid id, string email)
    {
        Payload = new UserCreatedEmailOutboxMessageV1Payload(id, email);
    }

    public string Type => OutboxMessageType;

    public UserCreatedEmailOutboxMessageV1Payload Payload { get; }

    public Task<string> SerializeAsync()
    {
        // => Serialize the payload to a string, which is deserialized in the appropriate IOutboxPublisher.
        // In a real world implementation this should use the ISerializer from the DataHub Serializer package.
        return Task.FromResult(JsonSerializer.Serialize(Payload));
    }
}
