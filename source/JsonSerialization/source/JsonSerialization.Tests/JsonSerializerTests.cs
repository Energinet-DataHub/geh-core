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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Xunit;
using Xunit.Categories;
using JsonSerializer = Energinet.DataHub.Core.JsonSerialization.JsonSerializer;

namespace JsonSerialization.Tests
{
    [UnitTest]
    public class JsonSerializerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void When_SerializeObjectWithInstant_Then_DeserializedString_ReturnsEqualInstant(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Instant.Should().BeEquivalentTo(message.Instant);
        }

        [Theory]
        [InlineAutoMoqData]
        public void When_SerializeObjectWithString_Then_DeserializedString_ReturnsEqualString(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.String.Should().Be(message.String);
        }

        [Theory]
        [InlineAutoMoqData]
        public void When_SerializeObjectWithDateTime_Then_DeserializedString_ReturnsEqualDateTime(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.DateTime.Should().BeSameDateAs(message.DateTime);
        }

        [Theory]
        [InlineAutoMoqData]
        public void When_SerializeObjectWithDecimal_Then_DeserializedString_ReturnsEqualDecimal(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Decimal.Should().Be(message.Decimal);
        }

        [Theory]
        [InlineAutoMoqData]
        public void When_SerializeObjectWithDouble_Then_DeserializedString_ReturnsEqualDouble(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Double.Should().Be(message.Double);
        }

        [Theory]
        [InlineAutoMoqData]
        public void When_SerializeObjectWithInt_Then_DeserializedString_ReturnsEqualInt(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var actual = sut.Serialize(message);
            var deserializedObject = sut.Deserialize<TestObject>(actual);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Int.Should().Be(message.Int);
        }

        [Theory]
        [InlineAutoMoqData]
        public void SerializeString_StringIsNull_ThrowsException(JsonSerializer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.Serialize((string)null!));
        }

        [Theory]
        [InlineAutoMoqData]
        public void Deserialize_JsonStringIsNull_ThrowsException(JsonSerializer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.Deserialize<TestObject>(null!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SerializeToStream(JsonSerializer sut, [Frozen] TestObject message)
        {
            // Act
            var stream = new MemoryStream();
            await sut.SerializeAsync(stream, message);
            var jsonFromStream = Encoding.UTF8.GetString(stream.ToArray());
            var deserializedObject = sut.Deserialize<TestObject>(jsonFromStream);

            // Assert
            deserializedObject.Should().NotBeNull();
            deserializedObject.Int.Should().Be(message.Int);
            deserializedObject.String.Should().Be(message.String);
            deserializedObject.Decimal.Should().Be(message.Decimal);
            deserializedObject.Double.Should().Be(message.Double);
            deserializedObject.Instant.Should().Be(message.Instant);
            deserializedObject.DateTime.Should().Be(message.DateTime);
        }
    }
}
