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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.TestCommon.Tests.Unit.AutoFixture.AttributeTests
{
    public class AutoMoqDataAttributeTests
    {
        [Theory]
        [AutoMoqData]
        public void When_ParameterInTestIsPresent_Then_TheyAreNotNull(
            [Frozen] AutoMoqObject autoMoqObject,
            AutoMoqClass sut)
        {
            // Assert
            autoMoqObject.Should().NotBeNull();
            sut.Should().NotBeNull();
        }

        [Theory]
        [AutoMoqData]
        public void When_ParameterInTestIsPresent_Then_SutCanBeUsed(
            [Frozen] AutoMoqObject autoMoqObject,
            AutoMoqClass sut)
        {
            // Arrange
            var addNumber = 1;
            var expected = autoMoqObject.Number + 1;

            // Act
            var result = sut.Add(autoMoqObject, addNumber);

            // Assert
            result.Number.Should().Be(expected);
        }
    }
}
