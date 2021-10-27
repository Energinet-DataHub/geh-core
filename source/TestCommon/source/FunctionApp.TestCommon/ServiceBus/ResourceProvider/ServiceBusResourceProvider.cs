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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider
{
    /// <summary>
    /// The resource provider and related builders encapsulates the creation of resources in an existing
    /// Azure Service Bus namespace, and support creating the related client types as well.
    ///
    /// The queue/topic names are build using a combination of the given name as well as a
    /// random suffix per provider instance. This ensures we can easily identity resources from a certain
    /// test run; and avoid name clashing if the test suite is executed by two identities at the same time.
    ///
    /// Disposing the service bus resource provider will delete all created resources and dispose any created clients.
    /// </summary>
    public class ServiceBusResourceProvider : IAsyncDisposable
    {
        /// <summary>
        /// If created topics/queues are not deleted explicit, they will automatically be deleted after this idle timeout.
        /// </summary>
        private static readonly TimeSpan AutoDeleteOnIdleTimeout = TimeSpan.FromMinutes(5);

        public ServiceBusResourceProvider(string connectionString)
        {
            ConnectionString = string.IsNullOrWhiteSpace(connectionString)
                ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString))
                : connectionString;

            AdministrationClient = new ServiceBusAdministrationClient(ConnectionString);
            Client = new ServiceBusClient(ConnectionString);

            RandomSuffix = $"{DateTimeOffset.UtcNow:yyyy.MM.ddTHH.mm.ss}-{Guid.NewGuid()}";
            QueueResources = new ConcurrentDictionary<string, QueueResource>();
        }

        public string ConnectionString { get; }

        /// <summary>
        /// Is used as part of the resource names.
        /// Allows us to identify resources created using the same instance (e.g. for debugging).
        /// </summary>
        public string RandomSuffix { get; }

        internal ServiceBusAdministrationClient AdministrationClient { get; }

        /// <summary>
        /// The client share its underlying connection with any senders/receivers created
        /// from it. Disposing the client will close the connection for all senders/receivers.
        /// See also: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/MigrationGuide.md#connection-pooling
        /// </summary>
        internal ServiceBusClient Client { get; }

        internal IDictionary<string, QueueResource> QueueResources { get; }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore()
                .ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Build a queue with a name based on <paramref name="queueNamePrefix"/> and <see cref="RandomSuffix"/>.
        /// </summary>
        /// <param name="queueNamePrefix">The queue name will start with this name.</param>
        /// <param name="maxDeliveryCount"></param>
        /// <param name="lockDuration"></param>
        /// <param name="requireSession"></param>
        /// <returns>Queue properties for the created queue.</returns>
        public QueueResourceBuilder BuildQueue(string queueNamePrefix, int maxDeliveryCount = 1, TimeSpan? lockDuration = null, bool requireSession = false)
        {
            var queueName = BuildResourceName(queueNamePrefix);
            var createQueueProperties = new CreateQueueOptions(queueName)
            {
                AutoDeleteOnIdle = AutoDeleteOnIdleTimeout,
                MaxDeliveryCount = maxDeliveryCount,
                LockDuration = lockDuration ?? TimeSpan.FromMinutes(1),
                RequiresSession = requireSession,
            };

            return new QueueResourceBuilder(this, createQueueProperties);
        }

        private string BuildResourceName(string namePrefix)
        {
            return string.IsNullOrWhiteSpace(namePrefix)
                ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(namePrefix))
                : $"{namePrefix}-{RandomSuffix}";
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            foreach (var queueResource in QueueResources)
            {
                // TODO: Dispose in parallel
                await queueResource.Value.DisposeAsync()
                    .ConfigureAwait(false);
            }

            // Disposing the client closes the underlying connection, which is also
            // used by any senders/receivers created with this client.
            await Client.DisposeAsync()
                .ConfigureAwait(false);
        }
    }
}
