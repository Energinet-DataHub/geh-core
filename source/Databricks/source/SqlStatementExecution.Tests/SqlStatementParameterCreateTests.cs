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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Tests;

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
using Xunit.Categories;

[UnitTest]
public class SqlStatementParameterCreateTests
{
    [Fact]
    public void CreateStringParameter_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = "someValue";

        // Act
        var actual = SqlStatementParameter.CreateStringParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("STRING");
    }

    [Fact]
    public void CreateIntParameter_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = 1;

        // Act
        var actual = SqlStatementParameter.CreateIntParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("INT");
    }

    [Fact]
    public void CreateLongParameter_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = 1L;

        // Act
        var actual = SqlStatementParameter.CreateLongParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("LONG");
    }

    [Fact]
    public void CreateDoubleParameter_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = 1.0;

        // Act
        var actual = SqlStatementParameter.CreateDoubleParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("DOUBLE");
    }

    [Fact]
    public void CreateDecimalParameter_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = 1.0m;

        // Act
        var actual = SqlStatementParameter.CreateDecimalParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("DECIMAL");
    }

    [Fact]
    public void CreateBooleanParameter_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = true;

        // Act
        var actual = SqlStatementParameter.CreateBooleanParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("BOOLEAN");
    }

    [Fact]
    public void CreateDate_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = new DateTime(2021, 1, 1);

        // Act
        var actual = SqlStatementParameter.CreateDateParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("DATE");
    }

    [Fact]
    public void CreateTimestamp_WhenCalled_ReturnsSqlStatementParameterWithExpectedName()
    {
        // Arrange
        var name = "someName";
        var value = new DateTime(2021, 1, 1, 12, 12, 12);

        // Act
        var actual = SqlStatementParameter.CreateTimestampParameter(name, value);

        // Assert
        actual.Name.Should().Be(name);
        actual.Value.Should().Be(value);
        actual.Type.Should().Be("TIMESTAMP");
    }
}
