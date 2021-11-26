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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation;
using SchemaValidation.Tests.Examples;
using Xunit;
using Xunit.Categories;

namespace SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderInvalidXmlTests
    {
        [Fact]
        public async Task AdvanceAsync_InvalidXml_ListsErrors()
        {
            // Arrange
            var xmlWithErrors = BookstoreExample
                .ExampleXml
                .Replace("genre=\"philosophy\"", string.Empty) // Remove attribute.
                .Replace("<title>The Autobiography of Benjamin Franklin</title>", string.Empty) // Remove node.
                .Replace("<price>11.99</price>", "<price invalidAttr=\"Invalid attribute.\">11.99</price>") // Add attribute.
                .Replace("<name>Plato</name>", "<name>Plato</name><unknown>Invalid node.</unknown>"); // Add node.

            var xmlStream = LoadStringIntoStream(xmlWithErrors);
            var target = new SchemaValidatingReader(xmlStream, new BookstoreExampleSchema());

            // Act
            while (await target.AdvanceAsync())
            { }

            // Assert
            Assert.True(target.HasErrors);
            Assert.Equal(4, target.Errors.Count);

            Assert.Contains(
                "The element 'book' in namespace 'http://www.contoso.com/books' has invalid child element 'author' in namespace 'http://www.contoso.com/books'. List of possible elements expected: 'title' in namespace 'http://www.contoso.com/books'.",
                target.Errors.Select(x => x.Description));

            Assert.Contains(
                "The required attribute 'genre' is missing.",
                target.Errors.Select(x => x.Description));

            Assert.Contains(
                "The 'invalidAttr' attribute is not declared.",
                target.Errors.Select(x => x.Description));

            Assert.Contains(
                "The element 'author' in namespace 'http://www.contoso.com/books' has invalid child element 'unknown' in namespace 'http://www.contoso.com/books'. List of possible elements expected: 'first-name, last-name' in namespace 'http://www.contoso.com/books'.",
                target.Errors.Select(x => x.Description));
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
