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

using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using ExampleHost.FunctionApp.Functions;
using ExampleHost.FunctionApp.IntegrationEvents.Contracts;
using ExampleHost.FunctionApp.Tests.Fixtures;
using FluentAssertions;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

[Collection(nameof(ExampleHostCollectionFixture))]
public class IntegrationEventsSubscriptionTests : FunctionAppTestBase<ExampleHostFixture>, IAsyncLifetime
{
    public IntegrationEventsSubscriptionTests(ExampleHostFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
        MessageFactory = new ServiceBusMessageFactory();
    }

    private ServiceBusMessageFactory MessageFactory { get; }

    public Task InitializeAsync()
    {
        Fixture.HostManager.ClearHostLog();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AcceptedEventHasAnyContent_WhenSend_ShouldHandleEvent()
    {
        // Arrange
        var acceptedEvent = new AcceptedV1
        {
            Content = "Any",
        };

        var message = MessageFactory.Create(
            new IntegrationEvent(
                EventIdentification: Guid.NewGuid(),
                EventName: acceptedEvent.GetType().Name,
                EventMinorVersion: 0,
                Message: acceptedEvent));

        // Act
        await Fixture.TopicResource.SenderClient.SendMessageAsync(message);

        // Assert
        await Fixture.HostManager.AssertFunctionWasExecutedAsync(nameof(IntegrationEventListener));
        Fixture.HostManager.WasMessageLogged("Event was handled").Should().BeTrue();
    }

    [Fact]
    public async Task AcceptedEventHasDeadLetterContent_WhenSend_EventShouldBeMovedToDeadLetterQueue()
    {
        // Arrange
        var acceptedEvent = new AcceptedV1
        {
            Content = "DeadLetter",
        };

        var message = MessageFactory.Create(
            new IntegrationEvent(
                EventIdentification: Guid.NewGuid(),
                EventName: acceptedEvent.GetType().Name,
                EventMinorVersion: 0,
                Message: acceptedEvent));

        // Act
        await Fixture.TopicResource.SenderClient.SendMessageAsync(message);

        // Assert
        await Fixture.HostManager.AssertFunctionWasExecutedAsync(nameof(IntegrationEventListener));
        Fixture.HostManager.CheckIfFunctionThrewException().Should().BeTrue();
        await Fixture.HostManager.AssertFunctionWasExecutedAsync(nameof(IntegrationEventDeadLetterListener));
    }

    [Fact]
    public async Task UnknownEvent_WhenSend_ShouldNotHandleEvent()
    {
        // Arrange
        var unknownEvent = new UnknownV1
        {
            Content = "Any",
        };

        var message = MessageFactory.Create(
            new IntegrationEvent(
                EventIdentification: Guid.NewGuid(),
                EventName: unknownEvent.GetType().Name,
                EventMinorVersion: 0,
                Message: unknownEvent));

        // Act
        await Fixture.TopicResource.SenderClient.SendMessageAsync(message);

        // Assert
        await Fixture.HostManager.AssertFunctionWasExecutedAsync(nameof(IntegrationEventListener));
        Fixture.HostManager.WasMessageLogged("Event was handled").Should().BeFalse();
    }
}
