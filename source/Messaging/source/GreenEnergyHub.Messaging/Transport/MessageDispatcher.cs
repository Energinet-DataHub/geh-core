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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Domain;

namespace GreenEnergyHub.Messaging.Transport
{
    /// <summary>
    /// A class that combines the serialized format, and the means of transport
    /// </summary>
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly MessageSerializer _serializer;
        private readonly Channel _channel;
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Construct a <see cref="MessageDispatcher"/>
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="channel">The channel where the data is sent to</param>
        /// <param name="jsonSerializer">The custom json serializer</param>
        public MessageDispatcher(MessageSerializer serializer, Channel channel, IJsonSerializer jsonSerializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <inheritdoc />
        public T Deserialize<T>(string str)
        {
            var res = _jsonSerializer.Deserialize<T>(str);
            return res;
        }

        /// <inheritdoc />
        public async Task DispatchAsync<T>(T message, string type, CancellationToken cancellationToken = default)
            where T : IOutboundMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var data = await _serializer.ToBytesAsync(message, type, cancellationToken).ConfigureAwait(false);

            await _channel.WriteToAsync(data, cancellationToken).ConfigureAwait(false);
        }
    }
}
