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
/// It encapsulates the name and value of the parameter. The <see cref="Type"/> property is set to "STRING" by default.
/// If another type is given, Databricks SQL Statement Execution API will perform type checking.
/// (See 'parameters' at https://docs.databricks.com/api/workspace/statementexecution/executestatement).
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
    /// Gets the data type of the SQL parameter, which is "STRING" by default.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStatementParameter"/> class
    /// with the specified name and value, and sets the data type to "STRING" by default.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The value of the SQL parameter.</param>
    /// <param name="type">The data type of the SQL parameter (default is "STRING").</param>
    public SqlStatementParameter(string name, string value, string type = "STRING")
    {
        Name = name;
        Value = value;
        Type = type;
    }
}
