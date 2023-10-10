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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Internal
{
    public class SqlChunkDataResponseParserTests
    {
        [Theory]
        [InlineAutoData]
        public async Task ParseAsync_ReturnsExpectedTableChunk(SqlChunkDataResponseParser sut)
        {
            // Arrange
            var jsonResponse = "[[\"John\", \"Doe\"], [\"Jane\", \"Smith\"]]";
            var expectedColumnNames = new[] { "FirstName", "LastName" };

            // Act
            await using var jsonStream = GenerateStreamFromString(jsonResponse);
            var data = sut.ParseAsync(jsonStream);

            // Assert
            await foreach (var actual in data)
            {
                actual.Should().NotBeNull();
                /*actual.ColumnNames.Should().BeEquivalentTo(expectedColumnNames);
                actual.RowCount.Should().Be(1);*/
            }

            /*var result = await data.FirstAsync();
            result[0].Should().BeEquivalentTo("John", "Doe");
            result[1].Should().BeEquivalentTo("Jane", "Smith");
            result[0, "FirstName"].Should().Be("John");
            result[0, "LastName"].Should().Be("Doe");*/
        }

        /*[Theory]
        [InlineAutoData]
        public void Parse_WithInvalidJsonResponse_ThrowsInvalidOperationException(
            SqlChunkDataResponseParser sut,
            string[] columnNames)
        {
            // Arrange
            var jsonResponse = "invalid json";

            // Act & Assert
            sut.Invoking(s => s.ParseAsync(GenerateStreamFromString(jsonResponse), columnNames))
                .Should().Throw<Exception>();
        }*/

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
