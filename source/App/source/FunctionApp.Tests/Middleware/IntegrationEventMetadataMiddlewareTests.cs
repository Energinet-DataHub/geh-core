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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.IntegrationEventContext;
using Energinet.DataHub.Core.App.FunctionApp.Tests.Common;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Middleware
{
    public sealed class IntegrationEventMetadataMiddlewareTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task Invoke_NotServiceBusTrigger_DoesNothing(string bindingType)
        {
            // Arrange
            var serializer = new JsonSerializer();
            var integrationEventContext = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            var target = new IntegrationEventMetadataMiddleware(
                serializer,
                integrationEventContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(bindingType).ToImmutableDictionary());

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            integrationEventContext
                .Invoking(c => c.ReadMetadata())
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task Invoke_NoUserProperties_DoesNothing()
        {
            // Arrange
            var serializer = new JsonSerializer();
            var integrationEventContext = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            var target = new IntegrationEventMetadataMiddleware(
                serializer,
                integrationEventContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.ServiceBusTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(new Dictionary<string, object?>());

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            integrationEventContext
                .Invoking(c => c.ReadMetadata())
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task Invoke_WrongUserPropertiesType_DoesNothing()
        {
            // Arrange
            var serializer = new JsonSerializer();
            var integrationEventContext = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            var target = new IntegrationEventMetadataMiddleware(
                serializer,
                integrationEventContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.ServiceBusTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(new Dictionary<string, object?> { { "UserProperties", new object() } });

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            integrationEventContext
                .Invoking(c => c.ReadMetadata())
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task Invoke_WithUserProperties_ReturnsMetadata(
            string messageType,
            Instant operationTimestamp)
        {
            // Arrange
            var serializer = new JsonSerializer();
            var integrationEventContext = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            var expected = new IntegrationEventJsonMetadata(
                messageType,
                operationTimestamp);

            var target = new IntegrationEventMetadataMiddleware(
                serializer,
                integrationEventContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.ServiceBusTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(SetupUserProperties(expected));

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            var actual = integrationEventContext.ReadMetadata();
            actual.MessageType.Should().Be(messageType);
            actual.OperationTimestamp.Should().Be(operationTimestamp);
        }

        private static IReadOnlyDictionary<string, BindingMetadata> SetupInputBindings(string bindingType)
        {
            var bindingMetadataMock = new Mock<BindingMetadata>();
            bindingMetadataMock.Setup(metadata => metadata.Type)
                .Returns(bindingType);

            var inputBindings = new Dictionary<string, BindingMetadata>
            {
                { "fake_value", bindingMetadataMock.Object },
            };
            return inputBindings;
        }

        private static IReadOnlyDictionary<string, object?> SetupUserProperties(IntegrationEventJsonMetadata mockedData)
        {
            var serializer = new JsonSerializer();
            var serialized = serializer.Serialize(mockedData);
            return new Dictionary<string, object?>
            {
                { "UserProperties", serialized },
            };
        }
    }
}
