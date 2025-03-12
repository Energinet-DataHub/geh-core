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

using Energinet.DataHub.Core.Databricks.Jobs.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.Jobs.Http;

/// <inheritdoc />
public class WorkspaceTokenProvider(IOptions<DatabricksJobsOptions> options, ILogger<WorkspaceTokenProvider> logger) : ITokenProvider
{
    /// <summary>
    /// Resolve a workspace token from the configuration.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Workspace token</returns>
    /// <exception cref="InvalidOperationException">If configuration does not contain a workspace token</exception>
    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(options?.Value?.WorkspaceToken)) return Task.FromResult(options.Value.WorkspaceToken);

        logger.LogWarning("Workspace token is missing.");
        throw new InvalidOperationException("Workspace token is missing.");
    }
}
