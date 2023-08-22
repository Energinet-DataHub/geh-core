﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Core.App.WebApp.Hosting;
using Energinet.DataHub.Core.Messaging.Communication.Subscriber;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

internal sealed class SubscriberTrigger : RepeatingTrigger<IIntegrationEventSubscriber>
{
    public SubscriberTrigger(
        IOptions<SubscriberWorkerOptions> options,
        IServiceProvider serviceProvider,
        ILogger<SubscriberTrigger> logger)
        : base(serviceProvider, logger, TimeSpan.FromMilliseconds(options.Value.HostedServiceExecutionDelayMs))
    {
    }

    protected override Task ExecuteAsync(IIntegrationEventSubscriber scopedService, CancellationToken cancellationToken, Action isAliveCallback)
    {
        return scopedService.ReceiveAsync(cancellationToken);
    }
}
