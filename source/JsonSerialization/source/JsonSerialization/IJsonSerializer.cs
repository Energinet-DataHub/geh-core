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

using System.Text.Json;

namespace Energinet.DataHub.Core.JsonSerialization
{
    /// <summary>
    /// Contract serialization and deserialization of JSON.
    /// Uses System.Text.Json to serialize and deserialize.
    /// Converter for NodaTime.Instant is registered by default.
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// Read the UTF-8 encoded text representing a single JSON value into a <paramref name="returnType"/>.
        /// The Stream will be read to completion.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the JSON value.</returns>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="utf8Json"/> or <paramref name="returnType"/> is null.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown when the JSON is invalid,
        /// the <paramref name="returnType"/> is not compatible with the JSON,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Thrown when the deserialized string returns null object.
        /// </exception>
        ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType);

        /// <summary>
        /// Parse the text representing a single JSON value into a <typeparamref name="TValue"/>.
        /// </summary>
        /// <returns>A <typeparamref name="TValue"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="json"/> is null.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown when the JSON is invalid,
        /// <typeparamref name="TValue"/> is not compatible with the JSON,
        /// or when there is remaining data in the Stream.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Thrown when the deserialized string returns null object.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        TValue Deserialize<TValue>(string json);

        /// <summary>
        /// Parse the text representing a single JSON value into an object of the type <paramref name="returnType"/>
        /// </summary>
        /// <returns>An object of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <param name="returnType">The type to parse the JSON value into</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="json"/> is null.
        /// </exception>
        /// <exception cref="JsonException">
        /// Thrown when the JSON is invalid,
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Thrown when the deserialized string returns null object.
        /// </exception>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using the
        /// UTF-8 methods since the implementation natively uses UTF-8.
        /// </remarks>
        object Deserialize(string json, Type returnType);

        /// <summary>
        /// Parse the value representing a single JSON value into an object of the type <typeparam name="T" />.
        /// </summary>
        Task<T> DeserializeAsync<T>(byte[] data);

        /// <summary>
        /// Convert the provided value into a <see cref="string"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="value">The value to convert.</param>
        /// <remarks>Using a <see cref="string"/> is not as efficient as using UTF-8
        /// encoding since the implementation internally uses UTF-8.
        /// </remarks>
        string Serialize<TValue>(TValue value);

        /// <summary>
        /// Convert the provided value into the input stream.
        /// </summary>
        /// <param name="stream">The stream to convert to.</param>
        /// <param name="value">The value to convert.</param>
        Task SerializeAsync<TValue>(Stream stream, TValue value);
    }
}
