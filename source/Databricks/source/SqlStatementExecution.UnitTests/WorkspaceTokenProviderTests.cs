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

using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests;

public class WorkspaceTokenProviderTests
{
    [Fact]
    public async Task Valid_configuration_returns_token()
    {
        var options = Options.Create(new DatabricksSqlStatementOptions
        {
            WorkspaceUrl = "https://example.databricks.com",
            WorkspaceToken = "your_token_here",
        });

        var logger = new LoggerFactory().CreateLogger<WorkspaceTokenProvider>();

        var sut = new WorkspaceTokenProvider(options, logger);
        var token = await sut.GetTokenAsync(CancellationToken.None);

        Assert.Equal(options.Value.WorkspaceToken, token);
    }

    [Fact]
    public async Task Invalid_configuration_throws_InvalidOperationException()
    {
        var options = Options.Create(new DatabricksSqlStatementOptions
        {
            WorkspaceUrl = "https://example.databricks.com",
            WorkspaceToken = string.Empty, // Invalid configuration
        });

        var logger = new LoggerFactory().CreateLogger<WorkspaceTokenProvider>();

        var sut = new WorkspaceTokenProvider(options, logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetTokenAsync(CancellationToken.None));
    }
}
