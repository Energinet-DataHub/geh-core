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
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.EventHub.ListenerMock
{
    public class DoProvider
    {
        internal DoProvider(EventHubListenerMock parent, Func<EventData, bool> eventMatcher)
        {
            Parent = parent;
            EventMatcher = eventMatcher;
        }

        private EventHubListenerMock Parent { get; }

        private Func<EventData, bool> EventMatcher { get; }

        /// <summary>
        /// Add message handler.
        /// </summary>
        public async Task<EventHubListenerMock> DoAsync(Func<EventData, Task> eventHandler)
        {
            await Parent.AddEventHandlerAsync(EventMatcher, eventHandler).ConfigureAwait(false);
            return Parent;
        }
    }
}
