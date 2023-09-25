﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Core.Databricks.Jobs.Internal;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.Internal;

public class JobsApiClientTests
{
    [Fact]
    public void Databricks_Jobs_Client_When_Calling_Jobs_Returns_Jobs_Api_Client()
    {
        // Arrange
        var options = new DatabricksJobsOptions { WorkspaceUrl = "https://test" };
        var sut = new JobsApiClient(Options.Create(options));

        // Act
        var actual = sut.Jobs;

        // Assert
        Assert.Equal(typeof(Microsoft.Azure.Databricks.Client.JobsApiClient), actual.GetType());
    }
}