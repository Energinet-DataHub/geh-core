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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderExtensionsTests
    {
        [Fact]
        public async Task AdvanceUntilClosedAsync_ElementReached_ReturnsFalse()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<root><test attr=""val""><other/></test></root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            while (await target.AdvanceUntilClosedAsync("test"))
            { }

            // Assert
            Assert.False(target.HasErrors);
            Assert.Equal("test", target.CurrentNodeName);
            Assert.Equal(NodeType.EndElement, target.CurrentNodeType);
        }

        [Fact]
        public async Task AdvanceUntilClosedAsync_ElementReached_CorredNodesVisited()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<root><test attr=""val""><other/></test></root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            var nodesToVisit = new HashSet<string>
            {
                "root",
                "test",
                "attr",
                "other",
            };

            // Act
            while (await target.AdvanceUntilClosedAsync("test"))
            {
                if (target.CurrentNodeType is NodeType.StartElement or NodeType.Attribute)
                {
                    Assert.True(nodesToVisit.Remove(target.CurrentNodeName));
                }
            }

            // Assert
            Assert.Empty(nodesToVisit);
            Assert.False(target.HasErrors);
        }

        [Fact]
        public async Task AdvanceUntilClosedAsync_NoElementReached_ReadsToEnd()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<root><test attr=""val""><other/></test></root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            while (await target.AdvanceUntilClosedAsync("not_there"))
            { }

            // Assert
            Assert.False(target.HasErrors);
            Assert.Equal(NodeType.None, target.CurrentNodeType);
        }

        [Fact]
        public async Task AsXElementAsync_ValidXml_ReadsToEnd()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<root><test attr=""val""><other/></test></root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            var xelement = await target.AsXElementAsync();

            // Assert
            Assert.NotNull(xelement);
            Assert.Equal("root", xelement.Name);
            Assert.False(target.HasErrors);
            Assert.Equal(NodeType.None, target.CurrentNodeType);
        }

        [Fact]
        public async Task AsXElementAsync_InvalidXml_HasErrors()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<wrong><test attr=""val""><other/></test></wrong>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act
            var xelement = await target.AsXElementAsync();

            // Assert
            Assert.NotNull(xelement);
            Assert.Equal("wrong", xelement.Name);
            Assert.True(target.HasErrors);
        }

        [Fact]
        public async Task AsXElementAsync_NotXml_ThrowsException()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<root> <<>> </root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            // Act + Assert
            await Assert.ThrowsAsync<XmlException>(() => target.AsXElementAsync());
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
