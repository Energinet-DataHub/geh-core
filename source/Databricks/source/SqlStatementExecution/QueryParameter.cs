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

using System.Text.Json.Serialization;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution;

public sealed class QueryParameter
{
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("value")]
    public string Value { get; }

    [JsonPropertyName("type")]
    public string Type { get; }

    private QueryParameter(string name, string value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Create a new QueryParameter with a name and value. Parameter type will be STRING.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns><see cref="QueryParameter"/></returns>
    public static QueryParameter Create(string name, string value) => new(name, value, "STRING");

    /// <summary>
    /// Create a new QueryParameter with a name and value as type.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns><see cref="QueryParameter"/></returns>
    public static QueryParameter Create(string name, string value, string type) => new(name, value, type);
}
