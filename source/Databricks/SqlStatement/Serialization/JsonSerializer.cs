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
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Energinet.DataHub.Core.Databricks.SqlStatement.Serialization;

public class JsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    static typeInfo =>
                    {
                        if (typeInfo.Kind != JsonTypeInfoKind.Object)
                        {
                            return;
                        }

                        foreach (var propertyInfo in typeInfo.Properties)
                        {
                            propertyInfo.IsRequired = true;
                        }
                    },
                },
            },
        };
    }

    public TValue Deserialize<TValue>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return System.Text.Json.JsonSerializer.Deserialize<TValue>(json, _options) ??
               throw new Exception($"Could not deserialize {json}");
    }
}
