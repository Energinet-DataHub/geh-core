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

using Energinet.DataHub.Core.Databricks.Jobs.Client;
using FluentAssertions;
using Moq;
using Xunit;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.Client;

public class JobsApiClientTests
{
    [Fact]
    public void Databricks_Jobs_Client_When_Calling_Jobs_Returns_Jobs_Api_Client()
    {
        // Arrange
        var httpClientFactoryMock = new Mock<IHttpClientFactory>().Object;
        var sut = new JobsApiClient(httpClientFactoryMock);

        // Act
        var actual = sut.Jobs;

        // Assert
        actual.GetType().Should().Be(typeof(Microsoft.Azure.Databricks.Client.JobsApiClient));
    }
}
