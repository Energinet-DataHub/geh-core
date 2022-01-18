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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Energinet.DataHub.Core.SchemaValidation.Tests.Examples;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.SchemaValidation.Tests
{
    [UnitTest]
    public sealed class SchemaValidatingReaderAsObjectTests
    {
        [Fact]
        public async Task Reconstruction_ValidXml_RebuildsObject()
        {
            // Arrange
            var origStream = LoadStreamIntoString(ExampleResources.BookstoreXml);
            var xmlStream = LoadStringIntoStream($"<root>{origStream}</root>");
            var target = new SchemaValidatingReader(xmlStream, new RootXmlSchema());

            Bookstore? bookstore = null;

            // Act
            while (await target.AdvanceAsync())
            {
                if (target.CurrentNodeName.Equals("bookstore", StringComparison.OrdinalIgnoreCase))
                {
                    if (target.CurrentNodeType == NodeType.EndElement)
                    {
                        continue;
                    }

                    bookstore = await ReadBookstoreAsync(target);
                }
            }

            // Assert
            Assert.False(target.HasErrors);

            var books = bookstore?.Books!;
            Assert.NotNull(books);

            var firstBook = books[0];
            Assert.Equal("The Autobiography of Benjamin Franklin", firstBook.Title);
            Assert.Equal("autobiography", firstBook.Genre);
            Assert.Equal("1-861003-11-0", firstBook.Isbn);
            Assert.Equal(8.99m, firstBook.Price);
            Assert.Equal(Instant.FromDateTimeOffset(new DateTimeOffset(1981, 03, 22, 0, 0, 0, TimeSpan.FromHours(0))), firstBook.PublicationDate);
            Assert.Equal("Benjamin", firstBook.Author?.FirstName);
            Assert.Equal("Franklin", firstBook.Author?.LastName);

            var secondBook = books[1];
            Assert.Equal("The Confidence Man", secondBook.Title);
            Assert.Equal("novel", secondBook.Genre);
            Assert.Equal("0-201-63361-2", secondBook.Isbn);
            Assert.Equal(11.99m, secondBook.Price);
            Assert.Equal(Instant.FromDateTimeOffset(new DateTimeOffset(1967, 11, 17, 0, 0, 0, TimeSpan.FromHours(0))), secondBook.PublicationDate);
            Assert.Equal("Herman", secondBook.Author?.FirstName);
            Assert.Equal("Melville", secondBook.Author?.LastName);

            var thirdBook = books[2];
            Assert.Equal("The Gorgias", thirdBook.Title);
            Assert.Equal("philosophy", thirdBook.Genre);
            Assert.Equal("1-861001-57-6", thirdBook.Isbn);
            Assert.Equal(9.99m, thirdBook.Price);
            Assert.Equal(Instant.FromDateTimeOffset(new DateTimeOffset(1991, 02, 15, 0, 0, 0, TimeSpan.FromHours(0))), thirdBook.PublicationDate);
            Assert.Equal("Plato", thirdBook.Author?.FirstName);
            Assert.Null(thirdBook.Author?.LastName);
        }

        private static async Task<Bookstore> ReadBookstoreAsync(ISchemaValidatingReader reader)
        {
            var books = new List<Book>();

            do
            {
                books.Add(await ReadBookAsync(reader));
            }
            while (await reader.AdvanceUntilClosedAsync("bookstore"));

            return new Bookstore { Books = books.ToArray() };
        }

        private static async Task<Book> ReadBookAsync(ISchemaValidatingReader reader)
        {
            var book = new Book();

            do
            {
                switch (reader.CurrentNodeName)
                {
                    case "ISBN":
                        book.Isbn = await reader.ReadValueAsStringAsync();
                        break;
                    case "title":
                        book.Title = await reader.ReadValueAsStringAsync();
                        break;
                    case "genre":
                        book.Genre = await reader.ReadValueAsStringAsync();
                        break;
                    case "author":
                        book.Author = await ReadAuthorAsync(reader);
                        break;
                    case "publicationdate":
                        book.PublicationDate = await reader.ReadValueAsNodaTimeAsync();
                        break;
                    case "price":
                        book.Price = await reader.ReadValueAsDecimalAsync();
                        break;
                }
            }
            while (await reader.AdvanceUntilClosedAsync("book"));

            return book;
        }

        private static async Task<Author> ReadAuthorAsync(ISchemaValidatingReader reader)
        {
            var author = new Author();

            do
            {
                switch (reader.CurrentNodeName)
                {
                    case "name":
                    case "first-name":
                        author.FirstName = await reader.ReadValueAsStringAsync();
                        break;
                    case "last-name":
                        author.LastName = await reader.ReadValueAsStringAsync();
                        break;
                }
            }
            while (await reader.AdvanceUntilClosedAsync("author"));

            return author;
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
