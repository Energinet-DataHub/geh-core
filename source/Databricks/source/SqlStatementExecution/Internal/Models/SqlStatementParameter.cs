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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
/// <summary>
/// Represents a parameter for a parameterized SQL query.
/// </summary>
/// <remarks>
/// The <see cref="SqlStatementParameter"/> class is used to define a parameter for a parameterized SQL query.
/// It encapsulates the name and value of the parameterThe <see cref="Type"/> property is always set to "STRING",
/// to avoid 3rd party type checking. If we were to provide types here, Statement Execution would perform type checking.
///
/// Instances of this class are typically used when constructing parameterized SQL
/// statements. See <see cref="DatabricksSqlStatementClient"/>
/// </remarks>
public sealed record SqlStatementParameter
{
    /// <summary>
    /// Gets the name of the SQL parameter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>
    /// Gets the value of the SQL parameter.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; }

    /// <summary>
    /// Gets the data type of the SQL parameter, which is always "STRING."
    /// </summary>
    [JsonPropertyName("type")]
    public static string Type => "STRING";

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStatementParameter"/> class
    /// with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The value of the SQL parameter.</param>
    public SqlStatementParameter(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
