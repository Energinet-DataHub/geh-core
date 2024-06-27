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

using System.Text;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Statement;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.Exceptions;

/// <summary>
/// Represents an exception that occurred during the execution of a Databricks statement.
/// </summary>
public sealed class DatabricksException : Exception
{
    internal DatabricksStatementRequest DatabricksStatementRequest { get; }

    internal DatabricksStatementResponse? Response { get; }

    internal DatabricksException(
        DatabricksStatementRequest databricksStatementRequest,
        DatabricksStatementResponse? response = null)
        : base(CreateErrorMessage(databricksStatementRequest, response))
    {
        DatabricksStatementRequest = databricksStatementRequest;
        Response = response;
    }

    private static string CreateErrorMessage(
        DatabricksStatementRequest databricksStatementRequest,
        DatabricksStatementResponse? response)
    {
        var errorMessage = new StringBuilder();
        errorMessage.AppendLine("An error occurred while executing a Databricks statement.");
        errorMessage.AppendLine("Statement:");
        errorMessage.AppendLine(databricksStatementRequest.Statement);

        var statusError = response?.status?.error;
        if (statusError != null)
        {
            if (statusError.error_code != null)
                errorMessage.AppendLine($"Response: ({statusError.error_code})");
            if (statusError.message != null)
                errorMessage.AppendLine(statusError.message);
        }

        return errorMessage.ToString();
    }
}
