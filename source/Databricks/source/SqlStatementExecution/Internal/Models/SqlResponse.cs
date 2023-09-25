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

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal.Models;

public class SqlResponse
{
    private SqlResponse(Guid statementId, SqlResponseState state, string[]? columnNames = null, SqlChunkResponse? chunk = null)
    {
        StatementId = statementId;
        State = state;
        ColumnNames = columnNames;
        Chunk = chunk;
    }

    public static SqlResponse CreateAsPending(Guid statementId)
    {
        return new SqlResponse(statementId, SqlResponseState.Pending);
    }

    public static SqlResponse CreateAsRunning(Guid statementId)
    {
        return new SqlResponse(statementId, SqlResponseState.Running);
    }

    public static SqlResponse CreateAsCancelled(Guid statementId)
    {
        return new SqlResponse(statementId, SqlResponseState.Cancelled);
    }

    public static SqlResponse CreateAsSucceeded(Guid statementId, string[] columnNames, SqlChunkResponse chunk)
    {
        return new SqlResponse(statementId, SqlResponseState.Succeeded, columnNames, chunk);
    }

    public static SqlResponse CreateAsFailed(Guid statementId)
    {
        return new SqlResponse(statementId, SqlResponseState.Failed);
    }

    public static SqlResponse CreateAsClosed(Guid statementId)
    {
        return new SqlResponse(statementId, SqlResponseState.Closed);
    }

    public Guid StatementId { get; }

    public SqlResponseState State { get; }

    public SqlChunkResponse? Chunk { get; }

    public string[]? ColumnNames { get; }
}
