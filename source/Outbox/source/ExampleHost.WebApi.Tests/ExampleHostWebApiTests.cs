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

using System.Net.Http.Json;
using ExampleHost.WebApi.Tests.Fixture;
using ExampleHost.WebApi.UserCreatedEmailOutboxMessage;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ExampleHost.WebApi.Tests;

public class ExampleHostWebApiTests : IClassFixture<OutboxTestFixture>, IAsyncLifetime
{
    private readonly OutboxTestFixture _fixture;

    public ExampleHostWebApiTests(OutboxTestFixture fixture)
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
    public async Task Given_UserCreatedRequestAddsMessageToOutbox_WhenOutboxProcessed_CorrectUserCreatedEmailOutboxMessageIsPublished()
    {
        // Arrange
        var createUserRequest = new HttpRequestMessage(HttpMethod.Post, "/User?email=test@test.com");

        var createUserResponse = await _fixture.WebApiClient.SendAsync(createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();

        // Act
        var runOutboxProcessorRequest = new HttpRequestMessage(HttpMethod.Post, "/OutboxProcessor/run");
        var runOutboxProcessorResponse = await _fixture.WebApiClient.SendAsync(runOutboxProcessorRequest);

        // Assert
        runOutboxProcessorResponse.EnsureSuccessStatusCode();

        await using var dbContext = _fixture.DatabaseManager.CreateDbContext();

        var outboxMessage = dbContext.Outbox.SingleOrDefault();
        outboxMessage.Should().NotBeNull();

        using var assertionScope = new AssertionScope();
        outboxMessage!.PublishedAt.Should().NotBeNull();
        outboxMessage.FailedAt.Should().BeNull();
        outboxMessage.Type.Should().Be(UserCreatedEmailOutboxMessageV1.OutboxMessageType);
        outboxMessage.Payload.Should().NotBeNullOrWhiteSpace();
    }
}
