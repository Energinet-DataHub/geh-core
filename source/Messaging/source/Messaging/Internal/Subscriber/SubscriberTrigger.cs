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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal.Subscriber;

internal sealed class SubscriberTrigger : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private IIntegrationEventSubscriber? _eventSubscriber;
    private IServiceScope? _scope;

    public SubscriberTrigger(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        if (_eventSubscriber is not null)
        {
            throw new InvalidOperationException($"This {nameof(SubscriberTrigger)} is already running");
        }

        _scope = _serviceProvider.CreateScope();
        _eventSubscriber = _scope.ServiceProvider.GetRequiredService<IIntegrationEventSubscriber>();
        return _eventSubscriber.StartAsync(stoppingToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_eventSubscriber is null)
        {
            return;
        }

        await _eventSubscriber.StopAsync(cancellationToken).ConfigureAwait(false);
        _scope!.Dispose();
        _eventSubscriber = null;
        _scope = null;
    }
}
