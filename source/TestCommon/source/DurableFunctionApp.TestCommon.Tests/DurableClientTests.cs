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
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Energinet.DataHub.Core.DurableFunctionApp.TestCommon.Tests;

[Collection(nameof(DurableTaskMockCollectionFixture))]
public class DurableClientTests(DurableTaskFixture fixture)
{
    [Fact]
    public async Task Given_WaitForOrchestrationStatusAsyncIsCalled_When_OrchestrationExists_Then_ReturnOrchestrationStatus()
    {
        // Arrange
        var createdTimeFrom = DateTime.UtcNow.AddMinutes(-5);
        var orchestrationName = "TestOrchestration";

        var mockStatus = new DurableOrchestrationStatus
        {
            Name = orchestrationName,
            RuntimeStatus = OrchestrationRuntimeStatus.Completed,
        };

        var mockClient = Mock.Get(fixture.MockDurableClient);
        mockClient.Setup(client => client.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None))
            .ReturnsAsync(new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = [mockStatus],
            });

        // Act
        var result = await fixture.MockDurableClient.WaitForOrchestrationStatusAsync(createdTimeFrom, orchestrationName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orchestrationName, result.Name);
        Assert.Equal(OrchestrationRuntimeStatus.Completed, result.RuntimeStatus);
    }

    [Fact]
    public async Task Given_WaitForInstanceCompletedAsyncIsCalled_When_OrchestrationFails_Then_ThrowException()
    {
        // Arrange
        var instanceId = "testInstanceId";
        var mockClient = Mock.Get(fixture.MockDurableClient);

        mockClient.Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Failed,
            });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            fixture.MockDurableClient.WaitForInstanceCompletedAsync(instanceId));
    }

    [Fact]
    public async Task Given_WaitForCustomStatusAsyncIsCalled_When_WhenCustomStatusMatches_Then_CustomStatusIsReturned()
    {
        // Arrange
        var instanceId = "testInstanceId";
        var expectedCustomStatus = new { State = "Ready" };
        var mockClient = Mock.Get(fixture.MockDurableClient);

        mockClient.Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                CustomStatus = JObject.FromObject(expectedCustomStatus),
                RuntimeStatus = OrchestrationRuntimeStatus.Running,
            });

        mockClient.Setup(client => client.GetStatusAsync(instanceId, true, true, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                CustomStatus = JObject.FromObject(expectedCustomStatus),
                RuntimeStatus = OrchestrationRuntimeStatus.Running,
            });

        // Act
        var result = await fixture.MockDurableClient.WaitForCustomStatusAsync<JObject>(instanceId, customStatus => customStatus["State"]?.ToString() == "Ready");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCustomStatus.State, result.CustomStatus["State"]?.ToString());
    }
}
