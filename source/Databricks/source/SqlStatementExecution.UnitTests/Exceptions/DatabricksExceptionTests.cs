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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Formats;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Exceptions;

[UnitTest]
public class DatabricksExceptionTests
{
    [Theory]
    [AutoMoqData]
    public void Constructor_WhenCalled_SetsMessage(DatabricksStatement statement)
    {
        // Arrange
        var databricksStatementRequest = new DatabricksStatementRequest("2", statement, Format.ApacheArrow.Value);
        var response = new DatabricksStatementResponse();

        // Act
        var actual = new DatabricksException(databricksStatementRequest, response);

        // Assert
        actual.Message.Should().NotBeEmpty();
        actual.DatabricksStatementRequest.Should().Be(databricksStatementRequest);
        actual.Response.Should().Be(response);
    }
}
