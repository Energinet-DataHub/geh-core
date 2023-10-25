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

// ReSharper disable InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable SA1300 // Element should begin with upper-case letter.
namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

/// <summary>
/// <see cref="DatabricksStatementResponse"/> is used for mapping json results,
/// hence we allow null values and inconsistent naming.
/// </summary>
internal class DatabricksStatementResponse
{
    public string? statement_id { get; set; }

    public Status? status { get; set; }

    public Manifest manifest { get; set; }

    public Result? result { get; set; }

    public bool IsSucceeded => status != null && status.state == "SUCCEEDED";

    public bool IsRunning => status != null && status.state == "RUNNING";

    public bool IsFailed => status != null && status.state == "FAILED";

    public bool IsCancelled => status != null && status.state == "CANCELLED";

    public bool IsPending => status != null && status.state == "PENDING";

    public bool IsClosed => status != null && status.state == "CLOSED";
}

internal class Status
{
    public string state { get; set; }
}

internal class Manifest
{
    public string format { get; set; }

    public Schema schema { get; set; }

    public int total_chunk_count { get; set; }

    public Chunks[] chunks { get; set; }

    public int total_row_count { get; set; }

    public int total_byte_count { get; set; }

    public bool truncated { get; set; }
}

internal class Schema
{
    public int column_count { get; set; }

    public Columns[] columns { get; set; }
}

internal class Columns
{
    public string name { get; set; }

    public string type_text { get; set; }

    public string type_name { get; set; }

    public int position { get; set; }

    public int type_precision { get; set; }

    public int type_scale { get; set; }
}

internal class Chunks
{
    public int chunk_index { get; set; }

    public int row_offset { get; set; }

    public int row_count { get; set; }

    public int byte_count { get; set; }
}

internal class Result
{
    public External_links[] external_links { get; set; }
}

internal class External_links
{
    public int chunk_index { get; set; }

    public int row_offset { get; set; }

    public int row_count { get; set; }

    public int byte_count { get; set; }

    public string external_link { get; set; }

    public string expiration { get; set; }
}

#pragma warning restore SA1300
#pragma warning restore CS8618
