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

using System.Collections.ObjectModel;
using Azure.Messaging.EventHubs.Producer;
using Azure.ResourceManager.EventHubs;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ResourceProvider;

public class EventHubResource : IAsyncDisposable
{
    private readonly Lazy<EventHubProducerClient> _lazyProducerClient;
    private readonly IList<EventHubsConsumerGroupResource> _consumerGroups;

    internal EventHubResource(EventHubResourceProvider resourceProvider, Azure.ResourceManager.EventHubs.EventHubResource innerResource)
    {
        ResourceProvider = resourceProvider;
        InnerResource = innerResource;

        _lazyProducerClient = new Lazy<EventHubProducerClient>(CreateProducerClient);
        _consumerGroups = [];

        ConsumerGroups = new ReadOnlyCollection<EventHubsConsumerGroupResource>(_consumerGroups);
    }

    public string ResourceGroup => ResourceProvider.ResourceManagementSettings.ResourceGroup;

    public string EventHubNamespace => ResourceProvider.FullyQualifiedNamespace;

    public string Name => InnerResource.Data.Name;

    public EventHubProducerClient ProducerClient => _lazyProducerClient.Value;

    public IReadOnlyCollection<EventHubsConsumerGroupResource>? ConsumerGroups { get; }

    public bool IsDisposed { get; private set; }

    internal Azure.ResourceManager.EventHubs.EventHubResource InnerResource { get; }

    private EventHubResourceProvider ResourceProvider { get; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore()
            .ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    internal void AddConsumerGroup(EventHubsConsumerGroupResource consumerGroup)
    {
        _consumerGroups.Add(consumerGroup);
    }

    private EventHubProducerClient CreateProducerClient()
    {
        return new EventHubProducerClient(ResourceProvider.FullyQualifiedNamespace, Name, ResourceProvider.Credential);
    }

    private async ValueTask DisposeAsyncCore()
    {
        if (IsDisposed)
        {
            return;
        }

        if (_lazyProducerClient.IsValueCreated)
        {
            await _lazyProducerClient.Value.DisposeAsync()
                .ConfigureAwait(false);
        }

        await InnerResource.DeleteAsync(Azure.WaitUntil.Completed).ConfigureAwait(false);

        IsDisposed = true;
    }
}
