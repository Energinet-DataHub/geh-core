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

using System.Net;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NodaTime;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests.Diagnostics.HealthChecks;

public class DatabricksSqlStatementApiHealthCheckTests
{
    [Theory]
    [InlineAutoMoqData(6, 20, 14, HealthStatus.Healthy, HttpStatusCode.OK)] // Healthy because inside interval and check was successful
    [InlineAutoMoqData(15, 20, 14, HealthStatus.Healthy, HttpStatusCode.OK)] // Healthy because outside interval (hours 15-20)
    [InlineAutoMoqData(14, 14, 14, HealthStatus.Healthy, HttpStatusCode.OK)] // Healthy because just inside interval and check was successful
    [InlineAutoMoqData(6, 20, 14, HealthStatus.Unhealthy, HttpStatusCode.BadRequest)] // Inside interval but unhealthy because check was unsuccessful
    public async Task Databricks_Interval_HealthCheck_When_Calling_Dependency_Returns_HealthStatus(
        int startHour,
        int endHour,
        int currentHour,
        HealthStatus expectedHealthStatus,
        HttpStatusCode httpStatusCode,
        [Frozen] Mock<IHttpClientFactory> httpClientFactoryMock,
        [Frozen] Mock<HttpMessageHandler> httpMessageHandlerMock,
        [Frozen] Mock<IClock> clockMock)
    {
        // Arrange
        var options = new DatabricksSqlStatementOptions
        {
            DatabricksHealthCheckStartHour = startHour,
            DatabricksHealthCheckEndHour = endHour,
            WorkspaceUrl = "https://fake",
        };
        var databricksOptions = Options.Create(options);

        clockMock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2021, 1, 1, currentHour, 0));
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(httpStatusCode));
        using var httpClientMock = new HttpClient(httpMessageHandlerMock.Object);
        httpClientFactoryMock.Setup(x => x.CreateClient(Options.DefaultName)).Returns(httpClientMock);
        var sut = new DatabricksSqlStatementApiHealthCheck(httpClientFactoryMock.Object, clockMock.Object, databricksOptions);

        // Act
        var actualHealthStatus = await sut
            .CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        actualHealthStatus.Status.Should().Be(expectedHealthStatus);
    }
}
