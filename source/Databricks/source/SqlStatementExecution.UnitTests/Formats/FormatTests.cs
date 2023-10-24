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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Formats;

[UnitTest]
public class FormatTests
{
    [Theory]
    [InlineData("arrow")]
    [InlineData("json")]
    public void JsonArray_MustBeJson(
        string formatKey)
    {
        // Arrange
        var format = new Format(formatKey);

        // Act
        var value = format.Value;

        // Assert
        value.Should().Be(formatKey);
    }

    [Theory]
    [InlineAutoMoqData("arrow", typeof(ApacheArrowFormat))]
    [InlineAutoMoqData("json", typeof(JsonArrayFormat))]
    public void GetStrategy_WithFormat_ReturnsStrategy(
        string formatKey,
        Type formatType,
        DatabricksSqlStatementOptions options)
    {
        // Arrange
        var format = new Format(formatKey);

        // Act
        var strategy = format.GetStrategy(options);

        // Assert
        strategy.GetType().Should().Be(formatType);
    }

    [Theory]
    [AutoMoqData]
    public void GetStrategy_UnknownFormat_ThrowsException(DatabricksSqlStatementOptions options)
    {
        // Arrange
        const string unknownFormat = "unknown";
        var format = new Format(unknownFormat);

        // Act
        var act = () => format.GetStrategy(options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Unknown format: {unknownFormat}");
    }
}
