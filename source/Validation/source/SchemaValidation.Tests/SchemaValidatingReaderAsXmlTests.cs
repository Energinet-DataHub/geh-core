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

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation.Tests.Examples;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderAsXmlTests
    {
        [Fact]
        public async Task Reconstruction_ValidXml_RebuildsXml()
        {
            // Arrange
            var origStream = LoadStreamIntoString(ExampleResources.BookstoreXml);
            var xmlStream = LoadStringIntoStream($"<root>{origStream}</root>");

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
            var expected = LoadStreamIntoString(ExampleResources.ReconstructedXml);
            // Remove copyright comment before comparison.
            expected = Regex.Replace(expected, "(?s)<!--.*?-->", string.Empty);
            Assert.Equal(expected, actual);
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }

        private static string LoadStreamIntoString(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
