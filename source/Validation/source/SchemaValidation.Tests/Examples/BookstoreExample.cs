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

using NodaTime;

namespace SchemaValidation.Tests.Examples
{
    internal static class BookstoreExample
    {
        public const string ExampleXml = @"
  <bookstore xmlns=""http://www.contoso.com/books"">
    <book genre=""autobiography"" publicationdate=""1981-03-22"" ISBN=""1-861003-11-0"">
      <title>The Autobiography of Benjamin Franklin</title>
      <author>
        <first-name>Benjamin</first-name>
        <last-name>Franklin</last-name>
      </author>
      <price>8.99</price>
    </book>
    <book genre=""novel"" publicationdate=""1967-11-17"" ISBN=""0-201-63361-2"">
      <title>The Confidence Man</title>
      <author>
        <first-name>Herman</first-name>
        <last-name>Melville</last-name>
      </author>
      <price>11.99</price>
    </book>
    <book genre=""philosophy"" publicationdate=""1991-02-15"" ISBN=""1-861001-57-6"">
      <title>The Gorgias</title>
      <author>
        <name>Plato</name>
      </author>
      <price>9.99</price>
    </book>
  </bookstore>";

        public sealed class Bookstore
        {
#pragma warning disable SA1011 // Conflicting rules: SA1011 wants Book[]? and SA1018 wants Book[] ?.
            public Book[]? Books { get; set; }
#pragma warning restore SA1011
        }

        public sealed class Book
        {
            public string? Isbn { get; set; }

            public string? Title { get; set; }

            public string? Genre { get; set; }

            public Author? Author { get; set; }

            public Instant? PublicationDate { get; set; }

            public decimal? Price { get; set; }
        }

        public sealed class Author
        {
            public string? FirstName { get; set; }

            public string? LastName { get; set; }
        }
    }
}
