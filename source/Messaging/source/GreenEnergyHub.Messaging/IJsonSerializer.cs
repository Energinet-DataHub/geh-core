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

using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GreenEnergyHub.Messaging
{
    /// <summary>
    /// IJsonSerializer wrapper for system.text.json.JsonSerializer
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Read the UTF-8 encoded text representing a single JSON value into a <typeparamref name="TValue"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="cancellationToken">
        /// The <see cref="System.Threading.CancellationToken"/> which may be used to cancel the read operation.
        /// </param>
        /// <exception cref="JsonException">
        /// Thrown when the JSON is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the JSON,
        /// or when there is remaining data in the Stream.
        /// </exception>
        public ValueTask<TValue?> DeserializeAsync<TValue>(
            Stream utf8Json,
            CancellationToken cancellationToken = default)
            where TValue : class;

        /// <summary>
        /// Read the UTF-8 encoded string JSON value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue"> representation of the JSON value.</typeparam>
        /// <param name="json"></param>
        /// <returns>Tvalue obj></returns>
        public TValue Deserialize<TValue>(string json);

        /// <summary>
        /// Convert the provided value into a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8. />
        /// </remarks>
        public string Serialize<TValue>(TValue value);
    }
}
