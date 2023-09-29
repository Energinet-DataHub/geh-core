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
/// It encapsulates the name, value and type of the parameter.
/// If another type than "STRING" is as type in the Create-method, Databricks SQL Statement Execution API will perform type checking.
/// (See <a href="https://docs.databricks.com/api/workspace/statementexecution/executestatement">'Parameters' section</a>)
/// </remarks>
public sealed record SqlStatementParameter
{
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("value")]
    public string Value { get; }

    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStatementParameter"/> class
    /// with the specified name, value and type.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The value of the SQL parameter.</param>
    /// <param name="type">The data type of the SQL parameter.</param>
    private SqlStatementParameter(string name, string value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/>.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The string value of the SQL parameter.</param>
    /// <param name="type">[Optional] The type of the SQL parameter. Default set to "STRING". </param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/>.</returns>
    public static SqlStatementParameter Create(string name, string value, string type = "STRING")
    {
        return new SqlStatementParameter(name, value, type);
    }
}
