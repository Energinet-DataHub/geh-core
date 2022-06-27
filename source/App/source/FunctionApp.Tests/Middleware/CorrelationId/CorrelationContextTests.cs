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
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.App.FunctionApp.Tests.Middleware.CorrelationId
{
    public class CorrelationContextTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Id_WhenSet_CanBeRetrieved([NotNull] CorrelationContext sut)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            sut.SetId(correlationId);

            // Act
            var result = sut.Id;

            // Assert
            correlationId.Should().Be(result);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Id_WhenNotSet_ThrowsException([NotNull] CorrelationContext sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Id);
        }
    }
}
