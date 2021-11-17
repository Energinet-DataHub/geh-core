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

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.EventHub.ListenerMock
{
    public class EventHubListenerMockTests
    {
        [Collection(nameof(EventHubListenerMockCollectionFixture))]
        public class InitializeAsync : EventHubListenerMockTestsBase
        {
            public InitializeAsync(EventHubListenerMockFixture listenerMockFixture)
                : base(listenerMockFixture)
            {
            }

            [Fact]
            public async Task When_EventHubExists_Then_EventsAreReceived()
            {
                // Arrange
                var eventHub = await ResourceProvider
                    .BuildEventHub("evh-01")
                    .CreateAsync();

                EventHubName = eventHub.Name;
                BlobContainerName = "container-01";

                // Act
                await Sut.InitializeAsync();

                // Assert
                var numberOfEvents = 3;
                using var eventBatch = await CreateFilledEventBatchAsync(eventHub.ProducerClient, numberOfEvents);
                await eventHub.ProducerClient.SendAsync(eventBatch);

                using var countDownEvent = new CountdownEvent(numberOfEvents);
                await Sut.AddEventHandlerAsync(
                    _ =>
                    {
                        return true;
                    },
                    _ =>
                    {
                        countDownEvent.Signal();
                        return Task.CompletedTask;
                    });

                var allWasReceived = countDownEvent.Wait(DefaultTimeout);
                allWasReceived.Should().BeTrue();

                Sut.ReceivedEvents.Count.Should().Be(numberOfEvents);
            }

            [Fact]
            public async Task When_EventHubNameDoesNotExist_Then_InvalidOperationExceptionIsThrown()
            {
                // Arrange
                EventHubName = "evh-02";
                BlobContainerName = "container-02";

                // Act
                Func<Task> act = () => Sut.InitializeAsync();

                // Assert
                await act.Should()
                    .ThrowAsync<EventHubsException>()
                    .WithMessage($"*{EventHubName}' could not be found*");
            }
        }

        [Collection(nameof(EventHubListenerMockCollectionFixture))]
        public class Reset : EventHubListenerMockTestsBase
        {
            public Reset(EventHubListenerMockFixture listenerMockFixture)
                : base(listenerMockFixture)
            {
            }

            [Fact]
            public async Task When_PreviouslyUsed_Then_InstanceCanBeReused()
            {
                // Arrange
                var eventHub = await ResourceProvider
                    .BuildEventHub("evh")
                    .CreateAsync();

                EventHubName = eventHub.Name;
                BlobContainerName = "container";

                await Sut.InitializeAsync();
                await AssertOneEventIsSendAndReceivedAsync(eventHub.ProducerClient);

                // Act
                Sut.Reset();

                // Assert
                await AssertOneEventIsSendAndReceivedAsync(eventHub.ProducerClient);
            }

            private async Task AssertOneEventIsSendAndReceivedAsync(EventHubProducerClient producerClient)
            {
                using var eventBatch = await CreateFilledEventBatchAsync(producerClient, numberOfEvents: 1);
                await producerClient.SendAsync(eventBatch);

                using var resetEvent = new ManualResetEventSlim(false);
                await Sut.AddEventHandlerAsync(
                    _ =>
                    {
                        return true;
                    },
                    _ =>
                    {
                        resetEvent.Set();
                        return Task.CompletedTask;
                    });

                var isReceived = resetEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();

                Sut.ReceivedEvents.Count.Should().Be(1);
            }
        }

        /// <summary>
        /// A new <see cref="EventHubListenerMock"/> is created and disposed for each test.
        /// Similar we create a new event hub for each test.
        /// </summary>
        public abstract class EventHubListenerMockTestsBase : TestBase<EventHubListenerMock>, IAsyncLifetime
        {
            protected EventHubListenerMockTestsBase(EventHubListenerMockFixture listenerMockFixture)
            {
                ListenerMockFixture = listenerMockFixture;
                ResourceProvider = new EventHubResourceProvider(
                    ListenerMockFixture.EventHubConnectionString,
                    ListenerMockFixture.ResourceManagementSettings,
                    ListenerMockFixture.TestLogger);
            }

            protected EventHubListenerMockFixture ListenerMockFixture { get; }

            protected EventHubResourceProvider ResourceProvider { get; }

            protected TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

            /// <summary>
            /// Set within test method to control creation of Sut.
            /// </summary>
            protected string EventHubName { get; set; }
                = string.Empty;

            /// <summary>
            /// Set within test method to control creation of Sut.
            /// </summary>
            protected string BlobContainerName { get; set; }
                = string.Empty;

            public Task InitializeAsync()
            {
                return OnInitializeAsync();
            }

            public async Task DisposeAsync()
            {
                await Sut.DisposeAsync();
                await ResourceProvider.DisposeAsync();
            }

            protected virtual Task OnInitializeAsync()
            {
                return Task.CompletedTask;
            }

            protected override EventHubListenerMock CreateSut()
            {
                Fixture.Inject<ITestDiagnosticsLogger>(ListenerMockFixture.TestLogger);
                Fixture.ForConstructorOn<EventHubListenerMock>()
                    .SetParameter("eventHubConnectionString").To(ListenerMockFixture.EventHubConnectionString)
                    .SetParameter("eventHubName").To(EventHubName)
                    .SetParameter("storageConnectionString").To(ListenerMockFixture.StorageConnectionString)
                    .SetParameter("blobContainerName").To(BlobContainerName);

                return base.CreateSut();
            }

            protected static async Task<EventDataBatch> CreateFilledEventBatchAsync(EventHubProducerClient producerClient, int numberOfEvents = 3)
            {
                var eventBatch = await producerClient.CreateBatchAsync();

                for (var i = 1; i <= numberOfEvents; i++)
                {
                    if (!eventBatch.TryAdd(new EventData($"Event {i}")))
                    {
                        // If it is too large for the batch
                        throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                    }
                }

                return eventBatch;
            }
        }
    }
}
