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

using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Databricks.Jobs.AppSettings;
using Energinet.DataHub.Core.Databricks.Jobs.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Core.Databricks.Jobs.UnitTests.Internal;

public class JobsApiClientTests
{
    /*
    [Theory]
    [InlineAutoMoqData(6, 20, 14, HealthStatus.Healthy)] // Healthy because inside interval and check was successful
    [InlineAutoMoqData(15, 20, 14, HealthStatus.Healthy)] // Healthy because outside interval (hours 15-20)
    [InlineAutoMoqData(14, 14, 14, HealthStatus.Healthy)] // Healthy because just inside interval and check was successful
    public async Task Databricks_Interval_HealthCheck_When_Calling_Dependency_Returns_Health_Status(
        int startHour,
        int endHour,
        int currentHour,
        HealthStatus expectedHealthStatus,
        Mock<HttpClient> httpClientMock)
    {
        // Arrange
        var options = new DatabricksJobsOptions
        {
            DatabricksHealthCheckStartHour = startHour,
            DatabricksHealthCheckEndHour = endHour,
        };
        clock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2021, 1, 1, currentHour, 0));
        jobsApiClientMock.Setup(x => x.Jobs).Returns(new Mock<IJobsApi>().Object);
        var sut = new JobsApiClient(httpClientMock,);

        // Act
        var actualHealthStatus = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None)
            .ConfigureAwait(false);

        // Assert
        actualHealthStatus.Status.Should().Be(expectedHealthStatus);
    }

    [Theory]
    [InlineAutoMoqData(6, 20, 14, 1)] // Inside interval
    [InlineAutoMoqData(16, 20, 14, 0)] // Outside interval
    public async Task Databricks_Interval_Health_Check_If_Calling_Dependency_Returns_HealthStatus(
        int startHour,
        int endHour,
        int currentHour,
        int times,
        Mock<IJobsApiClient> jobsApiClientMock,
        Mock<IClock> clock)
    {
        // Arrange
        var options = new DatabricksJobsOptions
        {
            DatabricksHealthCheckStartHour = startHour,
            DatabricksHealthCheckEndHour = endHour,
        };
        clock.Setup(x => x.GetCurrentInstant()).Returns(Instant.FromUtc(2021, 1, 1, currentHour, 0));
        jobsApiClientMock.Setup(x => x.Jobs).Returns(new Mock<IJobsApi>().Object);
        var sut = new DatabricksJobsApiHealthRegistration(jobsApiClientMock.Object, clock.Object, options);

        // Act
        var actualHealthStatus = await sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None)
            .ConfigureAwait(false);

        // Assert
        jobsApiClientMock.Verify(x => x.Jobs, Times.Exactly(times));
        actualHealthStatus.Status.Should().Be(HealthStatus.Healthy);
    }

    [Theory]
    [InlineAutoMoqData(-1, 20)]
    [InlineAutoMoqData(6, 24)]
    [InlineAutoMoqData(-1, 24)]
    public void Databricks_Interval_HealthCheck_When_Calling_Dependency_Throws_Invalid_Interval_Exception(
        int startHour,
        int endHour,
        [Frozen] Mock<IJobsApiClient> jobsApiClientMock,
        [Frozen] Mock<IClock> clockMock)
    {
        // Arrange
        var options = new DatabricksJobsOptions
        {
            DatabricksHealthCheckStartHour = startHour,
            DatabricksHealthCheckEndHour = endHour,
            WorkspaceUrl = "https://fake",
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DatabricksJobsApiHealthRegistration(jobsApiClientMock.Object, clockMock.Object, options));
    }*/
}
