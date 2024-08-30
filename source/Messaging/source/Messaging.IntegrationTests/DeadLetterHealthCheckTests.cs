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

using Messaging.IntegrationTests.Fixtures;

namespace Messaging.IntegrationTests;

public class DeadLetterHealthCheckTests(ServiceBusFixture fixture) : IClassFixture<ServiceBusFixture>, IAsyncLifetime
{
    private ServiceBusFixture Fixture { get; } = fixture;

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public void Can_create_topic_and_subscription()
    {
        Fixture.ServiceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();
    }
}
