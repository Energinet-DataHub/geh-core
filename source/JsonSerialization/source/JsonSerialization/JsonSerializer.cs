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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Energinet.DataHub.Core.JsonSerialization
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions();
            _options.Converters.Add(NodaConverters.InstantConverter);
            _options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(NodaConverters.InstantConverter);
            _options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public async ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);

            var result = await System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, _options).ConfigureAwait(false)
                ?? throw new NullReferenceException($"Deserialization the stream of type {nameof(returnType)} returned null");

            return result;
        }

        public TValue Deserialize<TValue>(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            var result = System.Text.Json.JsonSerializer.Deserialize<TValue>(json, _options)
                ?? throw new NullReferenceException("Deserialization of the string returned null");

            return result;
        }

        public object Deserialize(string json, Type returnType)
        {
            ArgumentNullException.ThrowIfNull(json);

            var result = System.Text.Json.JsonSerializer.Deserialize(json, returnType, _options)
                ?? throw new NullReferenceException($"Deserialization the string of type {nameof(returnType)} returned null");

            return result;
        }

        public async Task<T> DeserializeAsync<T>(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            var stream = new MemoryStream(data);
            await using (stream.ConfigureAwait(false))
            {
                return (T)await DeserializeAsync(stream, typeof(T)).ConfigureAwait(false);
            }
        }

        public string Serialize<TValue>(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return System.Text.Json.JsonSerializer.Serialize<object>(value, _options);
        }

        public Task SerializeAsync<TValue>(Stream stream, TValue value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return System.Text.Json.JsonSerializer.SerializeAsync<TValue>(stream, value, _options);
        }
    }
}
