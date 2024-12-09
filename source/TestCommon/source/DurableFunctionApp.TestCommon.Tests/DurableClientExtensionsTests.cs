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
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Energinet.DataHub.Core.DurableFunctionApp.TestCommon.Tests;

[Collection(nameof(DurableTaskCollectionFixture))]
public class DurableClientExtensionsTests(DurableTaskFixture fixture)
{
    [Fact]
    public async Task Given_WaitForInstanceCompletedAsyncIsCalled_When_OrchestrationFails_Then_ThrowException()
    {
        // Arrange
        var instanceId = "testInstanceId";
        var mockClient = Mock.Get(fixture.DurableClientMock);

        mockClient.Setup(client => client.GetStatusAsync(instanceId, false, false, true))
            .ReturnsAsync(new DurableOrchestrationStatus
            {
                RuntimeStatus = OrchestrationRuntimeStatus.Failed,
            });

        // Act
        var act = () => fixture.DurableClientMock.WaitForOrchestrationCompletedAsync(instanceId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Given_WaitForCustomStatusAsyncIsCalled_When_WhenCustomStatusMatches_Then_CustomStatusIsReturned()
    {
        // Arrange
        var instanceId = "testInstanceId";
        var expectedCustomStatus = new { State = "Ready" };
        var mockClient = Mock.Get(fixture.DurableClientMock);

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
        var result = await fixture.DurableClientMock.WaitForCustomStatusAsync<JObject>(
            instanceId,
            customStatus => customStatus["State"]?.ToString() == "Ready");

        // Assert
        result.Should().NotBeNull();
        result.CustomStatus["State"]?.ToString().Should().Be(expectedCustomStatus.State);
    }
}
