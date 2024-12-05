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
using Xunit;

namespace Energinet.DataHub.Core.DurableFunctionApp.TestCommon.Tests;

[Collection(nameof(DurableTaskCollectionFixture))]
public class DurableTaskManagerTests(DurableTaskFixture fixture)
{
    [Fact]
    public void When_TaskManagerIsInitialized_Then_ItShouldInitializeCorrectly()
    {
        // Assert
        fixture.DurableTaskManager.Should().NotBeNull();
        fixture.DurableTaskManager.ConnectionStringName.Should().Be("StorageConnectionString");
        fixture.DurableTaskManager.ConnectionString.Should().Be("UseDevelopmentStorage=true");
    }

    [Fact]
    public void When_CreateClientIsCalled_Then_ANewDurableTaskManagerIsReturned()
    {
        // Arrange
        var taskHubName = "TestHub";

        // Act
        var client = fixture.DurableTaskManager.CreateClient(taskHubName);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_DisposeAsyncIsCalled_When_DurableTaskManagerExists_Then_ItShouldBeDisposedProperly()
    {
        // Arrange
        var manager = new DurableTaskManager("ConnectionName", "UseDevelopmentStorage=true");

        // Act
        await manager.DisposeAsync();

        // Assert
        var act = () => manager.CreateClient("TestHub");
        act.Should().Throw<ObjectDisposedException>();
    }
}
