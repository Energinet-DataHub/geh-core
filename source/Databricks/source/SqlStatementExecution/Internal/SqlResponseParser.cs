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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Abstractions;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Models;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Internal;

public class SqlResponseParser : ISqlResponseParser
{
    private readonly ISqlStatusResponseParser _sqlStatusResponseParser;
    private readonly ISqlChunkResponseParser _sqlChunkResponseParser;
    private readonly ISqlChunkDataResponseParser _sqlChunkDataResponseParser;

    public SqlResponseParser(
        ISqlStatusResponseParser sqlStatusResponseParser,
        ISqlChunkResponseParser sqlChunkResponseParser,
        ISqlChunkDataResponseParser sqlChunkDataResponseParser)
    {
        _sqlStatusResponseParser = sqlStatusResponseParser;
        _sqlChunkResponseParser = sqlChunkResponseParser;
        _sqlChunkDataResponseParser = sqlChunkDataResponseParser;
    }

    public SqlResponse ParseStatusResponse(string jsonResponse)
    {
        return _sqlStatusResponseParser.Parse(jsonResponse);
    }

    public SqlChunkResponse ParseChunkResponse(string jsonResponse)
    {
        return _sqlChunkResponseParser.Parse(jsonResponse);
    }

    public IAsyncEnumerable<string[]?> ParseChunkDataResponseAsync(Stream jsonResponse, string[] columnNames)
    {
        return _sqlChunkDataResponseParser.ParseAsync(jsonResponse, columnNames);
    }
}
