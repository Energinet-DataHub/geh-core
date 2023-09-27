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

using Energinet.DataHub.Core.Databricks.Jobs.AppSettings;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.AppSettings;

public class OptionsTests
{
    [Theory]
    [InlineAutoMoqData(typeof(DatabricksJobsOptions), 5, "DATABRICKS_WORKSPACE_URL", "DATABRICKS_WORKSPACE_TOKEN", "DATABRICKS_WAREHOUSE_ID", "DATABRICKS_HEALTH_CHECK_START_HOUR", "DATABRICKS_HEALTH_CHECK_END_HOUR")]
    public void Options_HaveTheCorrectSettingNamesAndNumberOfSettings(Type sut, int settingsCount, params string[] expectedNames)
    {
        // Arrange & Act
        var properties = sut.GetProperties();

        // Assert
        properties.Length.Should().Be(settingsCount, $"the type {sut.Name}.");
        properties.Length.Should().Be(expectedNames.Length);
        foreach (var property in properties)
        {
            property.Name.Should().BeOneOf(expectedNames);
        }
    }
}
