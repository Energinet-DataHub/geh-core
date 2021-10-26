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
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus
{
    public class QueueResourceBuilder
    {
        public QueueResourceBuilder(ServiceBusManager serviceBusManager, QueueResource queueResource)
        {
            ServiceBusManager = serviceBusManager ?? throw new ArgumentNullException(nameof(serviceBusManager));
            QueueResource = queueResource ?? throw new ArgumentNullException(nameof(queueResource));
        }

        public string QueueName => QueueResource.Name;

        private QueueResource QueueResource { get; }

        private ServiceBusManager ServiceBusManager { get; }

        public ServiceBusSender CreateSenderClient()
        {
            if (QueueResource.SenderClient != null)
            {
                throw new InvalidOperationException("Sender client exists.");
            }

            QueueResource.SenderClient = ServiceBusManager.CreateSenderClient(QueueResource.Name);

            return QueueResource.SenderClient;
        }
    }
}
