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

using Energinet.DataHub.Core.Messaging.Communication;
using Energinet.DataHub.Core.Messaging.Communication.Publisher;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using FluentAssertions;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Core.Messaging.UnitTests;

[UnitTest]
public class ServiceProviderTests
{
    [Fact]
    public void ConfiguredServiceProvider_CanResolve_IPublisher()
    {
        var serviceProvider = CreateAndConfigureServiceCollectionDefaults()
            .Configure<PublisherOptions>(GetPublisherConfiguration())
            .AddPublisher<IntegrationEventProviderStub>()
            .BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetService<IPublisher>();

        publisher.Should().NotBeNull();
    }

    [Fact]
    public void ConfiguredServiceProvider_CanResolve_ISubscriber()
    {
        var serviceProvider = CreateAndConfigureServiceCollectionDefaults()
            .AddSubscriber<IntegrationEventHandlerStub>(Array.Empty<MessageDescriptor>())
            .BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var subscriber = scope.ServiceProvider.GetService<ISubscriber>();

        subscriber.Should().NotBeNull();
    }

    private static ServiceCollection CreateAndConfigureServiceCollectionDefaults()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        return serviceCollection;
    }

    private static IConfiguration GetPublisherConfiguration()
    {
        // This is as simple as it gets, but it is enough for the test to pass.
        // Extend this method to return a more realistic configuration if needed.
        var configuration = new ConfigurationBuilder()
            .Build();
        return configuration;
    }
}
