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
            Assert.Equal(messageType, target.EventMetadata.MessageType);
            Assert.Equal(operationTimestamp, target.EventMetadata.OperationTimestamp);
        }

        [Fact]
        public void EventMetadata_WhenNotSet_ThrowsException()
        {
            // Arrange
            var target = new FunctionApp.Middleware.IntegrationEventContext.IntegrationEventContext();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => target.EventMetadata);
        }
    }
}
