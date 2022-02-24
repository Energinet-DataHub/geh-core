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
using System.Text.Json;
using FluentAssertions;
using NodaTime;
using Xunit;
using JsonSerializer = Energinet.DataHub.Core.JsonSerialization.JsonSerializer;

namespace JsonSerialization.Tests
{
    public class JsonSerializerTests
    {
        [Fact]
        public void When_SerializeObjectWithInstant_Then_DeserializedString_ReturnsEqualInstant()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new TestObject
            {
                Instant = Instant.FromUnixTimeSeconds(1000),
            };

            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Instant.Should().BeEquivalentTo(message.Instant);
        }

        [Fact]
        public void When_SerializeObjectWithString_Then_DeserializedString_ReturnsEqualString()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new TestObject
            {
                String = "string",
            };

            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.String.Should().Be(message.String);
        }

        [Fact]
        public void When_SerializeObjectWithDateTime_Then_DeserializedString_ReturnsEqualDateTime()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new TestObject
            {
                DateTime = new DateTime(2020, 1, 1),
            };

            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.DateTime.Should().BeSameDateAs(message.DateTime);
        }

        [Fact]
        public void When_SerializeObjectWithDecimal_Then_DeserializedString_ReturnsEqualDecimal()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new TestObject
            {
                Decimal = 1.12m,
            };

            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Decimal.Should().Be(message.Decimal);
        }

        [Fact]
        public void When_SerializeObjectWithDouble_Then_DeserializedString_ReturnsEqualDouble()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new TestObject
            {
                Double = 3.45,
            };

            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Double.Should().Be(message.Double);
        }

        [Fact]
        public void When_SerializeObjectWithInt_Then_DeserializedString_ReturnsEqualInt()
        {
            // Arrange
            var sut = new JsonSerializer();
            var message = new TestObject
            {
                Int = 6,
            };

            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Int.Should().Be(message.Int);
        }

        [Fact]
        public void SerializeString_StringIsNull_ThrowsException()
        {
            var sut = new JsonSerializer();
            Assert.Throws<ArgumentNullException>(() => sut.Serialize((string)null));
        }

        [Fact]
        public void Deserialize_JsonStringIsNull_ThrowsException()
        {
            var sut = new JsonSerializer();
            Assert.Throws<ArgumentNullException>(() => sut.Deserialize<TestObject>(null!));
        }
    }
}
