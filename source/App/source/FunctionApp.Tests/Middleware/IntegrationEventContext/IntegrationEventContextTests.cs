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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Middleware.IntegrationEventContext
{
    public sealed class IntegrationEventContextTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void SetMetadata_WhenSet_CanBeRetrieved(
            string messageType,
            Instant operationTimestamp,
            string operationCorrelationId)
        {
            // Arrange
            var target = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            // Act
            target.SetMetadata(messageType, operationTimestamp, operationCorrelationId);

            // Assert
            var actual = target.ReadMetadata();
            actual.MessageType.Should().Be(messageType);
            actual.OperationTimestamp.Should().Be(operationTimestamp);
            actual.OperationCorrelationId.Should().Be(operationCorrelationId);
        }

        [Theory]
        [InlineAutoMoqData]
        public void TryReadMetadata_WhenSet_CanBeRetrieved(
            string messageType,
            Instant operationTimestamp,
            string operationCorrelationId)
        {
            // Arrange
            var target = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();
            target.SetMetadata(messageType, operationTimestamp, operationCorrelationId);

            // Act
            var result = target.TryReadMetadata(out var actual);

            // Assert
            result.Should().BeTrue();
            actual!.MessageType.Should().Be(messageType);
            actual.OperationTimestamp.Should().Be(operationTimestamp);
            actual.OperationCorrelationId.Should().Be(operationCorrelationId);
        }

        [Fact]
        public void TryReadMetadata_WhenNotSet_ReturnsFalse()
        {
            // Arrange
            var target = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            // Act
            var result = target.TryReadMetadata(out var actual);

            // Assert
            result.Should().BeFalse();
            actual.Should().BeNull();
        }

        [Fact]
        public void ReadMetadata_WhenNotSet_ThrowsException()
        {
            // Arrange
            var target = new App.Common.Abstractions.IntegrationEventContext.IntegrationEventContext();

            // Act + Assert
            target
                .Invoking(t => t.ReadMetadata())
                .Should()
                .Throw<InvalidOperationException>();
        }
    }
}
