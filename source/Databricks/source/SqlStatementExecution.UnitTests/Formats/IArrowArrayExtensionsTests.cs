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

using Apache.Arrow;
using Apache.Arrow.Types;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using FluentAssertions.Execution;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Formats;

public class IArrowArrayExtensionsTests
{
    [Fact]
    public void GetValue_WhenArrowArrayIsStructArray_ReturnsValue()
    {
        // Arrange
        var structField = new StructType(
            [
                new Field("id", new Int32Type(), nullable: false),
                new Field("timestamp", new TimestampType(TimeUnit.Second, TimeZoneInfo.Utc), nullable: false),
            ]);

        List<DateTimeOffset> dateTimeOffsets = [
            new DateTime(2021, 1, 1, 1, 1, 1),
            new DateTime(2022, 2, 2, 2, 2, 2),
        ];
        var timestampArray = new TimestampArray.Builder().AppendRange(dateTimeOffsets).Build();
        var intArray = new Int64Array.Builder().AppendRange([1, 2]).Build();
        var structArray = new StructArray(structField, 2, [intArray, timestampArray], ArrowBuffer.Empty, nullCount: 0);

        var recordBatch = new RecordBatch.Builder()
            .Append("element", false, structArray)
            .Build()
            .Arrays.First();

        // Act
        var result = recordBatch.GetValue(0);

        // Assert
        using var assertionScope = new AssertionScope();
        result.Should().NotBeNull();
        foreach (dynamic element in (IEnumerable<dynamic>)result!)
        {
            var id = (int)element.id;
            var timestamp = (DateTimeOffset)element.timestamp;

            id.Should().BeOneOf([1, 2]);
            timestamp.Should().BeOneOf(dateTimeOffsets);
        }
    }
}
