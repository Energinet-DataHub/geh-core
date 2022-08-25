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
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
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
    public sealed class CorrelationIdMiddlewareTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task Invoke_NotSupportedTrigger_DoesNothing(string bindingType)
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(serializer, correlationContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(bindingType).ToImmutableDictionary());

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            correlationContext
                .Invoking(c => c.Id)
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task Invoke_ServiceBusTriggerNoUserProperties_DoesNothing()
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

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
            correlationContext
                .Invoking(c => c.Id)
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task Invoke_ServiceBusTriggerWrongUserPropertiesType_DoesNothing()
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

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
            correlationContext
                .Invoking(c => c.Id)
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task Invoke_ServiceBusTriggerWithUserProperties_SetsCorrelationId(
            string messageType,
            Instant operationTimestamp,
            string operationCorrelationId)
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var expected = new IntegrationEventJsonMetadata(
                messageType,
                operationTimestamp,
                operationCorrelationId);

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

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
            var actual = correlationContext.Id;
            actual.Should().Be(operationCorrelationId);
        }

        [Fact]
        public async Task Invoke_HttpTriggerNoHeaders_DoesNothing()
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.HttpTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(new Dictionary<string, object?>());

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            correlationContext
                .Invoking(c => c.Id)
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task Invoke_HttpTriggerEmptyHeaders_DoesNothing()
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.ServiceBusTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(new Dictionary<string, object?> { { "Headers", new object() } });

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            correlationContext
                .Invoking(c => c.Id)
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task Invoke_HttpTriggerWithHeaders_SetsCorrelationId(string operationCorrelationId)
        {
            // Arrange
            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.HttpTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(SetupHeaders("CorrelationId", operationCorrelationId));

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            var actual = correlationContext.Id;
            actual.Should().Be(operationCorrelationId);
        }

        [Theory]
        [InlineData("CORRELATIONID")]
        [InlineData("correlationID")]
        [InlineData("CorrelationID")]
        [InlineData("correlationid")]
        public async Task Invoke_HttpTriggerWithDifferentCasing_SetsCorrelationId(string headerName)
        {
            // Arrange
            const string operationCorrelationId = "5F7FB6A2-350E-4E64-9E62-284ED435E0A7";

            var serializer = new JsonSerializer();
            var correlationContext = new CorrelationContext();

            var target = new CorrelationIdMiddleware(
                serializer,
                correlationContext);

            var context = new MockedFunctionContext();
            context.FunctionDefinitionMock
                .Setup(functionDefinition => functionDefinition.InputBindings)
                .Returns(SetupInputBindings(nameof(TriggerType.HttpTrigger)).ToImmutableDictionary());

            context.BindingContext
                .Setup(bindingContext => bindingContext.BindingData)
                .Returns(SetupHeaders(headerName, operationCorrelationId));

            // Act
            await target.Invoke(context.FunctionContext, _ => Task.CompletedTask);

            // Assert
            var actual = correlationContext.Id;
            actual.Should().Be(operationCorrelationId);
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

        private static IReadOnlyDictionary<string, object?> SetupHeaders(string headerName, string correlationId)
        {
            var serializer = new JsonSerializer();
            var serialized = serializer.Serialize(new Dictionary<string, string>
            {
                { headerName, correlationId },
            });

            return new Dictionary<string, object?>
            {
                { "Headers", serialized },
            };
        }
    }
}
