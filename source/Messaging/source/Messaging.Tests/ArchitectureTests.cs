// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using FluentAssertions;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Messaging.Tests;

[UnitTest]
public class ArchitectureTests
{
    [Fact]
    public void ServiceCollection_CallAddPublisher_IPublisherIsResolvable()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.Configure<PublisherOptions>(GetPublisherConfiguration());
        serviceCollection.AddPublisher<IntegrationEventProviderStub>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetService<IPublisher>();

        publisher.Should().NotBeNull();
    }

    [Fact]
    public void ServiceCollection_CallAddSubscriber_ISubscriberIsResolvable()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddSubscriber<IntegrationEventHandlerStub>(Array.Empty<MessageDescriptor>());
        var serviceProvider = serviceCollection.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var subscriber = scope.ServiceProvider.GetService<ISubscriber>();

        subscriber.Should().NotBeNull();
    }

    private static IConfiguration GetPublisherConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .Build();
        return configuration;
    }
}
