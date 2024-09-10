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

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.Core.TestCommon.Diagnostics;

namespace Energinet.DataHub.Core.Messaging.Communication.IntegrationTests.Fixtures;

public sealed class ServiceBusFixture : IAsyncLifetime
{
    public ServiceBusResourceProvider ServiceBusResourceProvider { get; } = new(
        new TestDiagnosticsLogger(),
        new IntegrationTestConfiguration().ServiceBusFullyQualifiedNamespace);

    public DefaultAzureCredential AzureCredential { get; } = new();

    public TopicResource? TopicResource { get; private set; }

    public QueueResource? QueueResource { get; private set; }

    public ServiceBusReceiver? TopicReceiver { get; private set; }

    public ServiceBusReceiver? TopicDeadLetterReceiver { get; private set; }

    public ServiceBusReceiver? QueueReceiver { get; private set; }

    public ServiceBusReceiver? QueueDeadLetterReceiver { get; private set; }

    private ServiceBusClient? Client { get; set; }

    public async Task InitializeAsync()
    {
        TopicResource = await ServiceBusResourceProvider
            .BuildTopic("The_Topic")
            .AddSubscription("The_Subscription")
            .CreateAsync();

        QueueResource = await ServiceBusResourceProvider
            .BuildQueue("The_Queue")
            .CreateAsync();

        Client = new ServiceBusClient(
            ServiceBusResourceProvider.FullyQualifiedNamespace,
            AzureCredential);

        TopicReceiver = Client.CreateReceiver(
            TopicResource.Name,
            TopicResource.Subscriptions.First().SubscriptionName);

        TopicDeadLetterReceiver = Client.CreateReceiver(
            TopicResource.Name,
            TopicResource.Subscriptions.First().SubscriptionName,
            new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

        QueueReceiver = Client.CreateReceiver(QueueResource.Name);

        QueueDeadLetterReceiver = Client.CreateReceiver(
            QueueResource.Name,
            new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });
    }

    public async Task DisposeAsync()
    {
        await ServiceBusResourceProvider.DisposeAsync();

        if (TopicReceiver is not null)
        {
            await TopicReceiver.DisposeAsync();
        }

        if (TopicDeadLetterReceiver is not null)
        {
            await TopicDeadLetterReceiver.DisposeAsync();
        }

        if (QueueReceiver is not null)
        {
            await QueueReceiver.DisposeAsync();
        }

        if (QueueDeadLetterReceiver is not null)
        {
            await QueueDeadLetterReceiver.DisposeAsync();
        }

        if (Client is not null)
        {
            await Client.DisposeAsync();
        }
    }
}
