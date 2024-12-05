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
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;

namespace Energinet.DataHub.Core.DurableFunctionApp.TestCommon.Tests.Fixtures;

public class DurableTaskFixture
{
    public DurableTaskFixture()
    {
        // Setup mocked IDurableClient
        var mockClient = new Mock<IDurableClient>();

        mockClient.Setup(client => client.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = [],
            });

        MockDurableClient = mockClient.Object;

        // Test

        // Setup DurableTaskManager
        TaskManager = new DurableTaskManager(
            "StorageConnectionString",
            "UseDevelopmentStorage=true");
    }

    public IDurableClient MockDurableClient { get; }

    public DurableTaskManager TaskManager { get; }

    public void Dispose()
    {
        TaskManager?.Dispose();
    }
}
