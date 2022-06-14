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
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Middleware.IntegrationEventContext
{
    public sealed class IntegrationEventContextTests
    {
        [Fact]
        public void SetMetadata_WhenSet_CanBeRetrieved()
        {
            const string messageType = "fake_value";
            var operationTimestamp = SystemClock.Instance.GetCurrentInstant();

            // Arrange
            var target = new FunctionApp.Middleware.IntegrationEventContext.IntegrationEventContext();

            // Act
            target.SetMetadata(messageType, operationTimestamp);

            // Assert
            var actual = target.ReadMetadata();
            Assert.Equal(messageType, actual.MessageType);
            Assert.Equal(operationTimestamp, actual.OperationTimestamp);
        }

        [Fact]
        public void TryReadMetadata_WhenSet_CanBeRetrieved()
        {
            const string messageType = "fake_value";
            var operationTimestamp = SystemClock.Instance.GetCurrentInstant();

            // Arrange
            var target = new FunctionApp.Middleware.IntegrationEventContext.IntegrationEventContext();
            target.SetMetadata(messageType, operationTimestamp);

            // Act
            var result = target.TryReadMetadata(out var actual);

            // Assert
            Assert.True(result);
            Assert.Equal(messageType, actual!.MessageType);
            Assert.Equal(operationTimestamp, actual.OperationTimestamp);
        }

        [Fact]
        public void TryReadMetadata_WhenNotSet_ReturnsFalse()
        {
            var operationTimestamp = SystemClock.Instance.GetCurrentInstant();

            // Arrange
            var target = new FunctionApp.Middleware.IntegrationEventContext.IntegrationEventContext();

            // Act
            var result = target.TryReadMetadata(out var actual);

            // Assert
            Assert.False(result);
            Assert.Null(actual);
        }

        [Fact]
        public void ReadMetadata_WhenNotSet_ThrowsException()
        {
            // Arrange
            var target = new FunctionApp.Middleware.IntegrationEventContext.IntegrationEventContext();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => target.ReadMetadata());
        }
    }
}
