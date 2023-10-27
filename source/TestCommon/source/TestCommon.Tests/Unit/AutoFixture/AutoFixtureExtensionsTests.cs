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

using AutoFixture;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Extensions;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Energinet.DataHub.Core.TestCommon.Tests.Unit.AutoFixture
{
    public class AutoFixtureExtensionsTests
    {
        public class ForConstructorOn
        {
            [Fact]
            public void When_SettingAllParameters_Then_ParameterValuesAreUsedForCreation()
            {
                // Arrange
                var sut = new Fixture();

                // Act
                var actual = sut.ForConstructorOn<HasNumberAndTest>()
                    .SetParameter("number").To(5)
                    .SetParameter("text").To("example text")
                    .Create();

                // Assert
                using var assertionScope = new AssertionScope();
                actual.Number.Should().Be(5);
                actual.Text.Should().Be("example text");
            }

            [Fact]
            public void When_SettingOneOfMultipleParameters_Then_ParameterValueIsUsedForCreation()
            {
                // Arrange
                var sut = new Fixture();

                // Act
                var actual = sut.ForConstructorOn<HasNumberAndTest>()
                    .SetParameter("number").To(5)
                    .Create();

                // Assert
                using var assertionScope = new AssertionScope();
                actual.Number.Should().Be(5);
                actual.Text.Should().NotBeNullOrEmpty();
            }

            [Theory]
            [InlineData(Priority.Second)]
            [InlineData(Priority.Third)]
            [InlineData(Priority.First)]
            public void When_SettingEnumParameter_Then_ParameterValueIsUsedForCreation(Priority priority)
            {
                // Arrange
                var sut = new Fixture();

                // Act
                var actual = sut.ForConstructorOn<HasEnum>()
                    .SetParameter("priority").To(priority)
                    .Create();

                // Assert
                actual.Priority.Should().Be(priority);
            }

            public class HasNumberAndTest
            {
                public HasNumberAndTest(int number, string text)
                {
                    Number = number;
                    Text = text;
                }

                public int Number { get; }

                public string Text { get; }
            }

            public class HasEnum
            {
                public HasEnum(Priority priority)
                {
                    Priority = priority;
                }

                public Priority Priority { get; }
            }

            public enum Priority
            {
                First = 0,
                Second = 1,
                Third = 2,
            }
        }
    }
}
