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
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;

public class TopicResource : IAsyncDisposable
{
    private readonly TopicProperties _properties;
    private readonly Lazy<ServiceBusSender> _lazySenderClient;
    private readonly IList<SubscriptionProperties> _subscriptions;

    internal TopicResource(ServiceBusResourceProvider resourceProvider, TopicProperties properties)
    {
        ResourceProvider = resourceProvider;

        _properties = properties;
        _lazySenderClient = new Lazy<ServiceBusSender>(CreateSenderClient);
        _subscriptions = new List<SubscriptionProperties>();

        Subscriptions = new ReadOnlyCollection<SubscriptionProperties>(_subscriptions);
    }

    public string Name => _properties.Name;

    public ServiceBusSender SenderClient => _lazySenderClient.Value;

    public IReadOnlyCollection<SubscriptionProperties> Subscriptions { get; }

    public bool IsDisposed { get; private set; }

    private ServiceBusResourceProvider ResourceProvider { get; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore()
            .ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    internal void AddSubscription(SubscriptionProperties subscriptionProperties)
    {
        _subscriptions.Add(subscriptionProperties);
    }

    private ServiceBusSender CreateSenderClient()
    {
        return ResourceProvider.Client.CreateSender(Name);
    }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
    private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
    {
        if (IsDisposed)
        {
            return;
        }

        if (_lazySenderClient.IsValueCreated)
        {
            await _lazySenderClient.Value.DisposeAsync()
                .ConfigureAwait(false);
        }

        await ResourceProvider.AdministrationClient.DeleteTopicAsync(Name)
            .ConfigureAwait(false);

        IsDisposed = true;
    }
}
