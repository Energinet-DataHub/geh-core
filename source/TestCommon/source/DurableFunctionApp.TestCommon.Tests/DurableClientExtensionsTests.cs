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

using Energinet.DataHub.Core.DurableFunctionApp.TestCommon.DurableTask;
using Energinet.DataHub.Core.DurableFunctionApp.TestCommon.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Energinet.DataHub.Core.DurableFunctionApp.TestCommon.Tests;

[Collection(nameof(DurableTaskCollectionFixture))]
public class DurableClientExtensionsTests(DurableTaskFixture fixture)
{
    [Fact]
    public async Task Given_WaitForOrchestrationCompletedAsyncIsCalled_When_OrchestrationFails_Then_ThrowException()
    {
        // Arrange
        var instanceId = Guid.NewGuid().ToString();
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Failed,
            });

        // Act
        var act = () => fixture.DurableClientMock.WaitForOrchestrationCompletedAsync(instanceId);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("*has an unexpected state. Actual state=`Failed`*");
    }

    [Fact]
    public async Task Given_WaitForOrchestrationCompletedAsyncIsCalled_When_OrchestrationPendingForEver_Then_ThrowException()
    {
        // Arrange
        var instanceId = Guid.NewGuid().ToString();
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Pending,
            });

        // Act
        var act = () => fixture.DurableClientMock.WaitForOrchestrationCompletedAsync(
            instanceId,
            waitTimeLimit: TimeSpan.FromSeconds(1));

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("*did not reach expected state within configured wait time limit. Actual state=`Pending`*");
    }

    [Fact]
    public async Task Given_WaitForOrchestrationCompletedAsyncIsCalled_When_OrchestrationCompleted_Then_OrchestrationStatusIsReturned()
    {
        // Arrange
        var instanceId = Guid.NewGuid().ToString();
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Completed,
            });

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, true, true, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                InstanceId = instanceId,
                RuntimeStatus = OrchestrationRuntimeStatus.Completed,
            });

        // Act
        var actual = await fixture.DurableClientMock.WaitForOrchestrationCompletedAsync(instanceId);

        // Assert
        using var assertionScope = new AssertionScope();
        actual.Should().NotBeNull();
        actual.InstanceId.Should().Be(instanceId);
        actual.RuntimeStatus.Should().Be(OrchestrationRuntimeStatus.Completed);
    }

    [Fact]
    public async Task Given_WaitForOrchestrationRunningAsyncIsCalled_When_OrchestrationTerminated_Then_ThrowException()
    {
        // Arrange
        var instanceId = Guid.NewGuid().ToString();
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Terminated,
            });

        // Act
        var act = () => fixture.DurableClientMock.WaitForOrchestrationRunningAsync(instanceId);

        // Assert
        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("*has an unexpected state. Actual state=`Terminated`*");
    }

    [Fact]
    public async Task Given_WaitForOrchestrationRunningAsyncIsCalled_When_OrchestrationRunning_Then_OrchestrationStatusIsReturned()
    {
        // Arrange
        var instanceId = Guid.NewGuid().ToString();
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Running,
            });

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, true, true, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                InstanceId = instanceId,
                RuntimeStatus = OrchestrationRuntimeStatus.Running,
            });

        // Act
        var actual = await fixture.DurableClientMock.WaitForOrchestrationRunningAsync(instanceId);

        // Assert
        actual.Should().NotBeNull();
        actual.InstanceId.Should().Be(instanceId);
        actual.RuntimeStatus.Should().Be(OrchestrationRuntimeStatus.Running);
    }

    [Fact]
    public async Task Given_WaitForCustomStatusAsyncIsCalled_When_WhenCustomStatusMatches_Then_CustomStatusIsReturned()
    {
        // Arrange
        var instanceId = Guid.NewGuid().ToString();
        var expectedCustomStatus = new { State = "Ready" };
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                CustomStatus = JObject.FromObject(expectedCustomStatus),
                RuntimeStatus = OrchestrationRuntimeStatus.Running,
            });

        mockClient
            .Setup(client => client.GetStatusAsync(instanceId, true, true, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                CustomStatus = JObject.FromObject(expectedCustomStatus),
                RuntimeStatus = OrchestrationRuntimeStatus.Running,
            });

        // Act
        var result = await fixture.DurableClientMock.WaitForCustomStatusAsync<JObject>(
            instanceId,
            customStatus => customStatus["State"]?.ToString() == "Ready");

        // Assert
        result.Should().NotBeNull();
        result.CustomStatus["State"]?.ToString().Should().Be(expectedCustomStatus.State);
    }
}
