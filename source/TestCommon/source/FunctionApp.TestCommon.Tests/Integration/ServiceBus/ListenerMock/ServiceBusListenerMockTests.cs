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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.ServiceBus.ListenerMock
{
    public class ServiceBusMockTests
    {
        [Collection(nameof(ServiceBusListenerMockCollectionFixture))]
        public class AddQueueListenerAsync : ServiceBusListenerMockTestsBase
        {
            public AddQueueListenerAsync(ServiceBusListenerMockFixture serviceBusListenerMockFixture)
                : base(serviceBusListenerMockFixture)
            {
            }

            [Fact]
            public async Task When_MessageIsSentToQueueName_Then_MessageIsReceived()
            {
                // Arrange
                await Sut.AddQueueListenerAsync(ServiceBusListenerMockFixture.Queue!.Name);

                var message = Fixture.Create<ServiceBusMessage>();

                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Queue.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_QueueNameDoesNotExist_Then_InvalidOperationExceptionIsThrown()
            {
                // Arrange

                // Act
                Func<Task> action = async () => await Sut.AddQueueListenerAsync(Fixture.Create<string>());

                // Assert
                await action.Should().ThrowAsync<InvalidOperationException>();
            }
        }

        [Collection(nameof(ServiceBusListenerMockCollectionFixture))]
        public class AddTopicSubscriptionListenerAsync : ServiceBusListenerMockTestsBase
        {
            public AddTopicSubscriptionListenerAsync(ServiceBusListenerMockFixture serviceBusListenerMockFixture)
                : base(serviceBusListenerMockFixture)
            {
            }

            [Fact]
            public async Task When_MessageIsSentToTopicName_Then_MessageIsReceived()
            {
                // Arrange
                await Sut.AddTopicSubscriptionListenerAsync(ServiceBusListenerMockFixture.Topic!.Name, ServiceBusListenerMockFixture.SubscriptionName);

                var message = Fixture.Create<ServiceBusMessage>();

                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Topic.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();
            }
        }

        [Collection(nameof(ServiceBusListenerMockCollectionFixture))]
        public class ResetMessageHandlersAndReceiveMessages : ServiceBusListenerMockTestsBase
        {
            public ResetMessageHandlersAndReceiveMessages(ServiceBusListenerMockFixture serviceBusListenerMockFixture)
                : base(serviceBusListenerMockFixture)
            {
            }

            [Fact]
            public async Task When_MessageHandlersAndMessagesReceivedIsReset_Then_InstanceCanBeReused()
            {
                // Arrange
                await Sut.AddQueueListenerAsync(ServiceBusListenerMockFixture.Queue!.Name);
                await CanReceiveMessageAsync();

                // Act
                Sut.ResetMessageHandlersAndReceivedMessages();

                // Assert
                var isReceived = await CanReceiveMessageAsync();
                isReceived.Should().BeTrue();

                Sut.ReceivedMessages.Count.Should().Be(1);
            }

            private async Task<bool> CanReceiveMessageAsync()
            {
                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                var message = Fixture.Create<ServiceBusMessage>();
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                return isReceivedEvent.Wait(DefaultTimeout);
            }
        }

        /// <summary>
        /// Test <see cref="WhenProvider.When(ServiceBusListenerMock, Func{ServiceBusReceivedMessage, bool})"/>
        /// and <see cref="DoProvider.DoAsync"/>,
        /// including related extensions.
        /// </summary>
        [Collection(nameof(ServiceBusListenerMockCollectionFixture))]
        public class WhenDoProviders : ServiceBusListenerMockTestsBase
        {
            /// <summary>
            /// Tests depends on the fact that a queue listener has been added in <see cref="OnInitializeAsync"/>.
            /// </summary>
            public WhenDoProviders(ServiceBusListenerMockFixture serviceBusListenerMockFixture)
                : base(serviceBusListenerMockFixture)
            {
            }

            [Fact]
            public async Task When_MessageMatch_Then_DoIsTriggered()
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();
                using var isReceivedEvent = new ManualResetEventSlim(false);

                await Sut.When(receivedMessage =>
                        receivedMessage.MessageId == message.MessageId
                        && receivedMessage.Subject == message.Subject)
                    .DoAsync(_ =>
                    {
                        isReceivedEvent.Set();
                        return Task.CompletedTask;
                    });

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_AnyMessageAlreadyReceived_Then_DoIsTriggered()
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);
                await Awaiter.WaitUntilConditionAsync(() => Sut.ReceivedMessages.Count == 1, TimeSpan.FromSeconds(5));

                // Act
                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_OneMessageAlreadyReceivedAndSecondMessageIsSentAfterSettingUpHandler_Then_DoIsTriggered()
            {
                // Arrange
                var message1 = Fixture.Create<ServiceBusMessage>();
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message1);
                await Awaiter.WaitUntilConditionAsync(() => Sut.ReceivedMessages.Count == 1, TimeSpan.FromSeconds(5));

                var messagesReceivedInHandler = new List<ServiceBusReceivedMessage>();
                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyCountAsync(
                        2,
                        receivedMessage =>
                        {
                            messagesReceivedInHandler.Add(receivedMessage);
                            return Task.CompletedTask;
                        });

                var message2 = Fixture.Create<ServiceBusMessage>();

                // Act
                await ServiceBusListenerMockFixture.Queue.SenderClient.SendMessageAsync(message2);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();

                messagesReceivedInHandler.Should()
                    .Contain(receivedMessage => receivedMessage.MessageId.Equals(message1.MessageId))
                    .And.Contain(receivedMessage => receivedMessage.MessageId.Equals(message2.MessageId));
            }

            [Fact]
            public async Task When_AnyMessage_Then_DoIsTriggered()
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();
                using var isReceivedEvent = new ManualResetEventSlim(false);

                await Sut.WhenAny()
                    .DoAsync(_ =>
                    {
                        isReceivedEvent.Set();
                        return Task.CompletedTask;
                    });

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_AnyMessage_Then_VerifyOnce()
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();

                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().BeTrue();
            }

            [Theory]
            [InlineData("123", "123", true)]
            [InlineData("123", "456", false)]
            public async Task When_MessageIdFilter_Then_VerifyOnceIfMatch(string messageId, string matchMessageId, bool expectDoIsTriggered)
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();
                message.MessageId = messageId;

                using var isReceivedEvent = await Sut
                    .WhenMessageId(matchMessageId)
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().Be(expectDoIsTriggered);
            }

            [Theory]
            [InlineData("123", "123", true)]
            [InlineData("123", "456", false)]
            public async Task When_MessageIdFilterAndMessageIsReplayed_Then_VerifyOnceIfMatch(string messageId, string matchMessageId, bool expectDoIsTriggered)
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();
                message.MessageId = messageId;

                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);
                await Awaiter.WaitUntilConditionAsync(() => Sut.ReceivedMessages.Count == 1, TimeSpan.FromSeconds(5));

                // Act
                using var isReceivedEvent = await Sut
                    .WhenMessageId(matchMessageId)
                    .VerifyOnceAsync();

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().Be(expectDoIsTriggered);
            }

            [Theory]
            [InlineData("PropelData", "PropelData", true)]
            [InlineData("PropelData", "MeteringData", false)]
            public async Task When_SubjectFilter_Then_VerifyOnceIfMatch(string messageSubject, string matchSubject, bool expectDoIsTriggered)
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();
                message.Subject = messageSubject;

                using var isReceivedEvent = await Sut
                    .WhenSubject(matchSubject)
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                var isReceived = isReceivedEvent.Wait(DefaultTimeout);
                isReceived.Should().Be(expectDoIsTriggered);
            }

            [Fact]
            public async Task When_AnyMessage_Then_VerifyCount()
            {
                // Arrange
                var expectedCount = 3;
                using var whenAllEvent = await Sut
                    .WhenAny()
                    .VerifyCountAsync(expectedCount);

                // Act
                for (var i = 0; i < expectedCount; i++)
                {
                    var message = Fixture.Create<ServiceBusMessage>();
                    _ = ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);
                }

                // Assert
                var allReceived = whenAllEvent.Wait(DefaultTimeout);
                allReceived.Should().BeTrue();
            }

            /// <summary>
            /// Preparing all <see cref="WhenDoProviders"/> tests with a queue listener.
            /// </summary>
            protected override Task OnInitializeAsync()
            {
                return Sut.AddQueueListenerAsync(ServiceBusListenerMockFixture.Queue!.Name);
            }
        }

        [Collection(nameof(ServiceBusListenerMockCollectionFixture))]
        public class ReceivedMessages : ServiceBusListenerMockTestsBase
        {
            /// <summary>
            /// Tests depends on the fact that a queue listener has been added in <see cref="OnInitializeAsync"/>.
            /// </summary>
            public ReceivedMessages(ServiceBusListenerMockFixture serviceBusListenerMockFixture)
                : base(serviceBusListenerMockFixture)
            {
            }

            [Fact]
            public async Task When_MessageIsSent_Then_ReceivedMessagesContainsExpectedMessage()
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();

                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                isReceivedEvent.Wait(DefaultTimeout);

                Sut.AssertReceived(receivedMessage =>
                    receivedMessage.MessageId == message.MessageId
                    && receivedMessage.Subject == message.Subject
                    && receivedMessage.CorrelationId == message.CorrelationId);
            }

            [Fact]
            public async Task When_MessageWithBodyIsSent_Then_ReceivedMessagesContainsMessageWithBodyAsString()
            {
                // Arrange
                var message = Fixture.Create<ServiceBusMessage>();

                using var isReceivedEvent = await Sut
                    .WhenAny()
                    .VerifyOnceAsync();

                // Act
                await ServiceBusListenerMockFixture.Queue!.SenderClient.SendMessageAsync(message);

                // Assert
                isReceivedEvent.Wait(DefaultTimeout);

                Sut.AssertReceived(receivedMessage =>
                    Encoding.UTF8.GetString(receivedMessage.Body) == DefaultBody);
            }

            /// <summary>
            /// Preparing all <see cref="ReceivedMessages"/> tests with a queue listener.
            /// </summary>
            protected override Task OnInitializeAsync()
            {
                return Sut.AddQueueListenerAsync(ServiceBusListenerMockFixture.Queue!.Name);
            }
        }

        /// <summary>
        /// A new <see cref="ServiceBusListenerMock"/> is created and disposed for each test.
        /// </summary>
        public abstract class ServiceBusListenerMockTestsBase : TestBase<ServiceBusListenerMock>, IAsyncLifetime
        {
            public const string DefaultBody = "valid body";

            protected ServiceBusListenerMockTestsBase(ServiceBusListenerMockFixture serviceBusListenerMockFixture)
            {
                ServiceBusListenerMockFixture = serviceBusListenerMockFixture;

                // Customize auto fixture
                Fixture.Inject<ITestDiagnosticsLogger>(ServiceBusListenerMockFixture.TestLogger);
                Fixture.ForConstructorOn<ServiceBusListenerMock>()
                    .SetParameter("connectionString").To(ServiceBusListenerMockFixture.ConnectionString);
                Fixture.Customize<ServiceBusMessage>(composer => composer
                    .OmitAutoProperties()
                    .With(p => p.MessageId)
                    .With(p => p.Subject)
                    .With(p => p.Body, new BinaryData(DefaultBody)));
            }

            protected ServiceBusListenerMockFixture ServiceBusListenerMockFixture { get; }

            protected TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

            public Task InitializeAsync()
            {
                return OnInitializeAsync();
            }

            public async Task DisposeAsync()
            {
                await OnDisposeAsync();
                await Sut.DisposeAsync();
            }

            protected virtual Task OnInitializeAsync()
            {
                return Task.CompletedTask;
            }

            protected virtual Task OnDisposeAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
