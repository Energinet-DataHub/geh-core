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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderErrorExtensionsTests
    {
        [Fact]
        public async Task CreateErrorResponse_HasError_ReturnsErrorResponse()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<wrong><other/></wrong>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            while (await target.AdvanceAsync())
            { }

            // Act
            var actual = target.CreateErrorResponse();

            // Assert
            Assert.Equal("B2B-005", actual.Error.Code);
            Assert.Equal("The specified input does not pass schema validation.", actual.Error.Message);

            var details = actual.Error.Details!;
            Assert.NotNull(details);

            var single = details.Single();
            Assert.Equal("SchemaValidationError", single.Code);
            Assert.Equal("The 'wrong' element is not declared.", single.Message);
            Assert.Equal(1, single.InnerError?.LineNumber);
            Assert.Equal(2, single.InnerError?.LinePosition);
        }

        [Fact]
        public async Task CreateErrorResponse_HasManyErrors_ReturnsErrors()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<bookstore xmlns=""http://www.contoso.com/books""><book /></bookstore>");
            var target = new SchemaValidatingReader(xmlStream, new StreamSchema(Examples.ExampleResources.BookstoreSchema));

            while (await target.AdvanceAsync())
            { }

            // Act
            var actual = target.CreateErrorResponse();

            // Assert
            var details = actual.Error.Details!;
            Assert.NotNull(details);
            Assert.Equal(4, details.Count());
        }

        [Fact]
        public async Task CreateErrorResponse_NoErrors_ThrowsException()
        {
            // Arrange
            var xmlStream = LoadStringIntoStream(@"<root><other/></root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            while (await target.AdvanceAsync())
            { }

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => target.CreateErrorResponse());
        }

        [Fact]
        public async Task WriteAsXmlAsync_HasError_WritesXml()
        {
            // Arrange
            var destination = new MemoryStream();
            var xmlStream = LoadStringIntoStream(@"<wrong><other/></wrong>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            while (await target.AdvanceAsync())
            { }

            var errorResponse = target.CreateErrorResponse();

            // Act
            await errorResponse.WriteAsXmlAsync(destination);

            // Assert
            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Error>\r\n  <Code>B2B-005</Code>\r\n  <Message>The specified input does not pass schema validation.</Message>\r\n  <Details>\r\n    <Error>\r\n      <Code>SchemaValidationError</Code>\r\n      <Message>The 'wrong' element is not declared.</Message>\r\n      <InnerError>\r\n        <LineNumber>1</LineNumber>\r\n        <LinePosition>2</LinePosition>\r\n      </InnerError>\r\n    </Error>\r\n  </Details>\r\n</Error>";

            destination.Position = 0;

            var actual = await new StreamReader(destination).ReadToEndAsync();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task WriteAsXmlAsync_HasManyErrors_WritesXml()
        {
            // Arrange
            var destination = new MemoryStream();
            var xmlStream = LoadStringIntoStream(@"<bookstore xmlns=""http://www.contoso.com/books""><book /></bookstore>");
            var target = new SchemaValidatingReader(xmlStream, new StreamSchema(Examples.ExampleResources.BookstoreSchema));

            while (await target.AdvanceAsync())
            { }

            var errorResponse = target.CreateErrorResponse();

            // Act
            await errorResponse.WriteAsXmlAsync(destination);

            // Assert
            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Error>\r\n  <Code>B2B-005</Code>\r\n  <Message>The specified input does not pass schema validation.</Message>\r\n  <Details>\r\n    <Error>\r\n      <Code>SchemaValidationError</Code>\r\n      <Message>The required attribute 'genre' is missing.</Message>\r\n      <InnerError>\r\n        <LineNumber>1</LineNumber>\r\n        <LinePosition>50</LinePosition>\r\n      </InnerError>\r\n    </Error>\r\n    <Error>\r\n      <Code>SchemaValidationError</Code>\r\n      <Message>The required attribute 'publicationdate' is missing.</Message>\r\n      <InnerError>\r\n        <LineNumber>1</LineNumber>\r\n        <LinePosition>50</LinePosition>\r\n      </InnerError>\r\n    </Error>\r\n    <Error>\r\n      <Code>SchemaValidationError</Code>\r\n      <Message>The required attribute 'ISBN' is missing.</Message>\r\n      <InnerError>\r\n        <LineNumber>1</LineNumber>\r\n        <LinePosition>50</LinePosition>\r\n      </InnerError>\r\n    </Error>\r\n    <Error>\r\n      <Code>SchemaValidationError</Code>\r\n      <Message>The element 'book' in namespace 'http://www.contoso.com/books' has incomplete content. List of possible elements expected: 'title' in namespace 'http://www.contoso.com/books'.</Message>\r\n      <InnerError>\r\n        <LineNumber>1</LineNumber>\r\n        <LinePosition>50</LinePosition>\r\n      </InnerError>\r\n    </Error>\r\n  </Details>\r\n</Error>";

            destination.Position = 0;

            var actual = await new StreamReader(destination).ReadToEndAsync();
            Assert.Equal(expected, actual);
        }

        private static Stream LoadStringIntoStream(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
