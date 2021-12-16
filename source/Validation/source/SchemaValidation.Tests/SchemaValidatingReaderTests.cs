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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Energinet.DataHub.Core.SchemaValidation.Tests.Examples;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderTests
    {
        [Fact]
        public void Ctor_NullStream_ThrowsException()
        {
            // Arrange + Act + Assert
            Assert.Throws<ArgumentNullException>(() => new SchemaValidatingReader(null!, new RootXmlSchema()));
        }

        [Fact]
        public void Ctor_NullSchema_ThrowsException()
        {
            // Arrange + Act + Assert
            Assert.Throws<ArgumentNullException>(() => new SchemaValidatingReader(new MemoryStream(), null!));
        }

        [Fact]
        public void AdvanceAsync_ValidXml_CorrectInitialValues()
        {
            // Arrange
            const string xml = "<root />";
            var xmlStream = LoadStringIntoStream(xml);

            // Act
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Assert
            Assert.Empty(target.CurrentNodeName);
            Assert.Equal(NodeType.None, target.CurrentNodeType);
            Assert.False(target.CanReadValue);
            Assert.False(target.HasErrors);
            Assert.Empty(target.Errors);
        }

        [Fact]
        public async Task AdvanceAsync_NotXml_ReturnsAsError()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream("<root>< <> ></root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            await target.AdvanceAsync();

            // Assert
            Assert.True(target.HasErrors);

            var error = target.Errors.Single();
            Assert.Equal(1, error.LineNumber);
            Assert.Equal(8, error.LinePosition);
            Assert.Equal("Name cannot begin with the ' ' character, hexadecimal value 0x20. Line 1, position 8.", error.Description);
        }

        [Fact]
        public async Task AdvanceAsync_InvalidSchema_ThrowsException()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream("<root><content /></root>");
            var target = new SchemaValidatingReader(xmlStream, new StreamSchema(ExampleResources.BrokenSchema));

            // Act + Assert
            await Assert.ThrowsAsync<XmlSchemaException>(() => target.AdvanceAsync());
        }

        [Fact]
        public async Task AdvanceAsync_ValidXml_CorrectFinalValues()
        {
            // Arrange
            const string xml = "<root />";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            var resultA = await target.AdvanceAsync();
            var resultB = await target.AdvanceAsync();

            // Assert
            Assert.True(resultA);
            Assert.True(resultB);
            Assert.False(await target.AdvanceAsync());
            Assert.Empty(target.CurrentNodeName);
            Assert.Equal(NodeType.None, target.CurrentNodeType);
            Assert.False(target.CanReadValue);
            Assert.False(target.HasErrors);
            Assert.Empty(target.Errors);
        }

        [Fact]
        public async Task AdvanceAsync_ValidXml_ReadsNode()
        {
            // Arrange
            const string xml = "<root><content /></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            var result = await target.AdvanceAsync();

            // Assert
            Assert.True(result);
            Assert.Equal("root", target.CurrentNodeName);
            Assert.Equal(NodeType.StartElement, target.CurrentNodeType);
            Assert.False(target.CanReadValue);
            Assert.False(target.HasErrors);
        }

        [Fact]
        public async Task AdvanceAsync_ValidXml_ReadsEndNode()
        {
            // Arrange
            const string xml = "<root><content /></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            var resultA = await target.AdvanceAsync(); // <root>
            var resultB = await target.AdvanceAsync(); // <content>
            var resultC = await target.AdvanceAsync(); // </content>
            var resultD = await target.AdvanceAsync(); // </root>

            // Assert
            Assert.True(resultA);
            Assert.True(resultB);
            Assert.True(resultC);
            Assert.True(resultD);
            Assert.Equal("root", target.CurrentNodeName);
            Assert.Equal(NodeType.EndElement, target.CurrentNodeType);
            Assert.False(target.CanReadValue);
            Assert.False(target.HasErrors);
            Assert.False(await target.AdvanceAsync());
        }

        [Fact]
        public async Task AdvanceAsync_ValidXml_ReadsAttributes()
        {
            // Arrange
            const string xml = @"<root><content attr1=""attr1Value"" attr2=""attr2Value"" /></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            var resultA = await target.AdvanceAsync(); // <root>
            var resultB = await target.AdvanceAsync(); // <content>
            var resultC = await target.AdvanceAsync(); // <attr1>

            // Assert
            Assert.True(resultA);
            Assert.True(resultB);
            Assert.True(resultC);
            Assert.Equal("attr1", target.CurrentNodeName);
            Assert.Equal(NodeType.Attribute, target.CurrentNodeType);
            Assert.True(target.CanReadValue);
            Assert.Equal("attr1Value", await target.ReadValueAsStringAsync());
            Assert.False(target.HasErrors);

            // Act
            var resultD = await target.AdvanceAsync(); // <attr2>

            // Assert
            Assert.True(resultD);
            Assert.Equal("attr2", target.CurrentNodeName);
            Assert.Equal(NodeType.Attribute, target.CurrentNodeType);
            Assert.True(target.CanReadValue);
            Assert.Equal("attr2Value", await target.ReadValueAsStringAsync());
            Assert.False(target.HasErrors);
        }

        [Fact]
        public async Task ReadValueAsStringAsync_Content_ReturnsValue()
        {
            // Arrange
            const string xml = @"<root>expected</root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsStringAsync();

            // Assert
            Assert.Equal("expected", actual);
        }

        [Fact]
        public async Task ReadValueAsStringAsync_Attribute_ReturnsValue()
        {
            // Arrange
            const string xml = @"<root attribute=""expected""></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsStringAsync();

            // Assert
            Assert.Equal("expected", actual);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public async Task ReadValueAsBoolAsync_Content_ReturnsValue(string expected)
        {
            // Arrange
            var xml = @$"<root>{expected}</root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsBoolAsync();

            // Assert
            Assert.Equal(expected == "true", actual);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public async Task ReadValueAsBoolAsync_Attribute_ReturnsValue(string expected)
        {
            // Arrange
            var xml = @$"<root attribute=""{expected}""></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsBoolAsync();

            // Assert
            Assert.Equal(expected == "true", actual);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task ReadValueAsIntAsync_Content_ReturnsValue(int expected)
        {
            // Arrange
            var xml = @$"<root>{expected}</root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsIntAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task ReadValueAsIntAsync_Attribute_ReturnsValue(int expected)
        {
            // Arrange
            var xml = @$"<root attribute=""{expected}""></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsIntAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public async Task ReadValueAsLongAsync_Content_ReturnsValue(int expected)
        {
            // Arrange
            var xml = @$"<root>{expected}</root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsLongAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(long.MaxValue)]
        public async Task ReadValueAsLongAsync_Attribute_ReturnsValue(long expected)
        {
            // Arrange
            var xml = @$"<root attribute=""{expected}""></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsLongAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1111)]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(1111)]
        public async Task ReadValueAsDecimalAsync_Content_ReturnsValue(decimal expected)
        {
            if (expected == 1111)
            {
                expected = decimal.MaxValue;
            }

            if (expected == -1111)
            {
                expected = decimal.MinValue;
            }

            // Arrange
            var xml = @$"<root>{expected}</root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsDecimalAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1111)]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(1111)]
        public async Task ReadValueAsDecimalAsync_Attribute_ReturnsValue(decimal expected)
        {
            if (expected == 1111)
            {
                expected = decimal.MaxValue;
            }

            if (expected == -1111)
            {
                expected = decimal.MinValue;
            }

            // Arrange
            var xml = @$"<root attribute=""{expected}""></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsDecimalAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2011-11-11T15:05:00+01:00")]
        [InlineData("1989-09-17")]
        public async Task ReadValueAsNodaTimeAsync_Content_ReturnsValue(string inputTime)
        {
            var expected = Instant.FromDateTimeOffset(DateTimeOffset.Parse(inputTime, CultureInfo.InvariantCulture));

            // Arrange
            var xml = @$"<root>{inputTime}</root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsNodaTimeAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2011-11-11T15:05:00+01:00")]
        [InlineData("1989-09-17")]
        public async Task ReadValueAsNodaTimeAsync_Attribute_ReturnsValue(string inputTime)
        {
            var expected = Instant.FromDateTimeOffset(DateTimeOffset.Parse(inputTime, CultureInfo.InvariantCulture));

            // Arrange
            var xml = @$"<root attribute=""{inputTime}""></root>";
            var xmlStream = LoadStringIntoStream(xml);

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            await target.AdvanceAsync();
            await target.AdvanceAsync();

            // Act
            var actual = await target.ReadValueAsNodaTimeAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
