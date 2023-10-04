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

using System.Net.Http.Headers;
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Configuration;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.IntegrationTests.Fixtures;

public static class HttpClientFactory
{
    public static HttpClient CreateHttpClient(DatabricksSqlStatementOptions databricksOptions)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(databricksOptions.WorkspaceUrl),
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", databricksOptions.WorkspaceToken);

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

        return httpClient;
    }
}
