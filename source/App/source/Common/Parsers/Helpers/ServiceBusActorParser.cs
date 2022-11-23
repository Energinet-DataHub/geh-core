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
using System.Linq;
using System.Text.Json;
using Energinet.DataHub.Core.App.Common.Abstractions.Actors;

namespace Energinet.DataHub.Core.App.Common.Parsers.Helpers
{
    public static class ServiceBusActorParser
    {
        public static Actor? FromDictionaryString(string inputText, string propertyKey)
        {
            if (string.IsNullOrWhiteSpace(inputText)) throw new ArgumentNullException(nameof(inputText));
            if (string.IsNullOrWhiteSpace(propertyKey)) throw new ArgumentNullException(nameof(propertyKey));

            var inputJsonDocument = JsonDocument.Parse(inputText);
            var resultJsonProperty = inputJsonDocument.RootElement
                .EnumerateObject()
                .FirstOrDefault(e => e.Name.Equals(propertyKey, StringComparison.Ordinal));

            return resultJsonProperty.Value.ValueKind == JsonValueKind.Undefined
                ? null
                : FromString(resultJsonProperty.Value.ToString() ?? string.Empty);
        }

        private static Actor? FromString(string userIdentity)
        {
            if (string.IsNullOrWhiteSpace(userIdentity)) throw new ArgumentNullException(nameof(userIdentity));

            return JsonSerializer.Deserialize<Actor>(userIdentity) ?? throw new JsonException(nameof(userIdentity));
        }
    }
}
