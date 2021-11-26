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
using Energinet.DataHub.Core.Schemas;
using Energinet.DataHub.Core.SchemaValidation;
using SchemaValidation.Tests.Examples;
using Xunit;
using Xunit.Categories;

namespace SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderAsXmlTests
    {
        private const string ReconstructedXml = @"<root><bookstore xmlns=""http://www.contoso.com/books""><book genre=""autobiography"" publicationdate=""1981-03-22"" ISBN=""1-861003-11-0""><title>The Autobiography of Benjamin Franklin</title><author><first-name>Benjamin</first-name><last-name>Franklin</last-name></author><price>8.99</price></book><book genre=""novel"" publicationdate=""1967-11-17"" ISBN=""0-201-63361-2""><title>The Confidence Man</title><author><first-name>Herman</first-name><last-name>Melville</last-name></author><price>11.99</price></book><book genre=""philosophy"" publicationdate=""1991-02-15"" ISBN=""1-861001-57-6""><title>The Gorgias</title><author><name>Plato</name></author><price>9.99</price></book></bookstore></root>";

        [Fact]
        public async Task Reconstruction_ValidXml_RebuildsXml()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream($"<root>{BookstoreExample.ExampleXml}</root>");

            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());
            var builder = new StringBuilder();
            var openTag = false;

            // Act
            while (await target.AdvanceAsync())
            {
                switch (target.CurrentNodeType)
                {
                    case NodeType.StartElement:

                        if (openTag)
                        {
                            builder.Append('>');
                            openTag = false;
                        }

                        if (target.CanReadValue)
                        {
                            builder.AppendFormat("<{0}>{1}", target.CurrentNodeName, await target.ReadValueAsStringAsync());
                        }
                        else
                        {
                            builder.AppendFormat("<{0}", target.CurrentNodeName);
                            openTag = true;
                        }

                        break;
                    case NodeType.EndElement:

                        if (openTag)
                        {
                            builder.Append('>');
                            openTag = false;
                        }

                        builder.AppendFormat("</{0}>", target.CurrentNodeName);
                        break;
                    case NodeType.Attribute:
                        builder.AppendFormat(" {0}=\"{1}\"", target.CurrentNodeName, await target.ReadValueAsStringAsync());
                        break;
                }
            }

            // Assert
            Assert.False(target.HasErrors);
            var actual = builder.ToString();
            Assert.Equal(ReconstructedXml, actual);
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
