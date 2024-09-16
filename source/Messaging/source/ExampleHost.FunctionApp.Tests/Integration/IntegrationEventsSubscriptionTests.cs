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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using ExampleHost.FunctionApp.Functions;
using ExampleHost.FunctionApp.Tests.Fixtures;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

[Collection(nameof(ExampleHostCollectionFixture))]
public class IntegrationEventsSubscriptionTests : FunctionAppTestBase<ExampleHostFixture>, IAsyncLifetime
{
    public IntegrationEventsSubscriptionTests(ExampleHostFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

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
    public async Task IntegrationEvent_WhenSend_ShouldTriggerIntegrationEventListener()
    {
        // Arrange
        var serviceBusMessage = new ServiceBusMessage()
        {
            MessageId = "messageId",
        };

        // Act
        await Fixture.TopicResource.SenderClient.SendMessageAsync(serviceBusMessage);

        // Assert
        await Fixture.HostManager.AssertFunctionWasExecutedAsync(nameof(IntegrationEventListener));
    }
}
