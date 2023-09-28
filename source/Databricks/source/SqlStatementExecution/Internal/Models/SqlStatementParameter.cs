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
using System.Text.Json.Serialization;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;
/// <summary>
/// Represents a parameter for a parameterized SQL query.
/// </summary>
/// <remarks>
/// The <see cref="SqlStatementParameter"/> class is used to define a parameter for a parameterized SQL query.
/// It encapsulates the name, value and type of the parameter.
/// If another type than "STRING" is given, Databricks SQL Statement Execution API will perform type checking.
/// (See <a href="https://docs.databricks.com/api/workspace/statementexecution/executestatement">'Parameters' section</a>)
/// </remarks>
public sealed record SqlStatementParameter
{
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("value")]
    public object Value { get; }

    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStatementParameter"/> class
    /// with the specified name, value and type.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The value of the SQL parameter.</param>
    /// <param name="type">The data type of the SQL parameter.</param>
    private SqlStatementParameter(string name, object value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a string value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The string value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a string value.</returns>
    public static SqlStatementParameter CreateStringParameter(string name, string value)
    {
        return new SqlStatementParameter(name, value, "STRING");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with an integer value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The integer value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with an integer value.</returns>
    public static SqlStatementParameter CreateIntParameter(string name, int value)
    {
        return new SqlStatementParameter(name, value, "INT");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a long value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The long value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a long value.</returns>
    public static SqlStatementParameter CreateLongParameter(string name, long value)
    {
        return new SqlStatementParameter(name, value, "LONG");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a double value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The double value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a double value.</returns>
    public static SqlStatementParameter CreateDoubleParameter(string name, double value)
    {
        return new SqlStatementParameter(name, value, "DOUBLE");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a double value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The double value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a decimal value.</returns>
    public static SqlStatementParameter CreateDecimalParameter(string name, decimal value)
    {
        return new SqlStatementParameter(name, value, "DECIMAL");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a boolean value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The boolean value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a boolean value.</returns>
    public static SqlStatementParameter CreateBooleanParameter(string name, bool value)
    {
        return new SqlStatementParameter(name, value, "BOOLEAN");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a date value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The date value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a date value.</returns>
    public static SqlStatementParameter CreateDateParameter(string name, DateTime value)
    {
        return new SqlStatementParameter(name, value, "DATE");
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementParameter"/> with a timestamp value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The timestamp value of the SQL parameter.</param>
    /// <returns>A new instance of <see cref="SqlStatementParameter"/> with a timestamp value.</returns>
    public static SqlStatementParameter CreateTimestampParameter(string name, DateTime value)
    {
        return new SqlStatementParameter(name, value, "TIMESTAMP");
    }
}
