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
/// This class is used to represent a parameter in a SQL statement, for the
/// Databricks Sql Statement Execution API.
///
/// A parameter consists of a name, a value, and optionally a type. To represent a NULL value, the value field
/// may be omitted or set to null explicitly. If the type field is omitted, the value is interpreted as a string.
/// </summary>
public record SqlStatementParameter([property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type);
