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
using Energinet.DataHub.Core.Databricks.SqlStatementExecution.Http;

namespace Energinet.DataHub.Core.Databricks.SqlStatementExecution.UnitTests;

public class AuthenticateRequestWithTokenTests
{
    private class StringTokenProvider(string token) : ITokenProvider
    {
        public Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(token);
        }
    }

    [Fact]
    public async Task SendAsync_ShouldAssignBearerTokenToRequest()
    {
        // Arrange
        var handler = CreateTokenHandler();

        var httpClient = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task SendAsync_ShouldReplaceDefaultBearerToken()
    {
        // Arrange
        var handler = CreateTokenHandler();

        var httpClient = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "default-token");

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("test-token", request.Headers.Authorization?.Parameter);
    }

    private static AuthenticateRequestWithToken CreateTokenHandler()
    {
        var tokenProvider = new StringTokenProvider("test-token");

        var handler = new AuthenticateRequestWithToken(tokenProvider)
        {
            InnerHandler = new HttpClientHandler(),
        };
        return handler;
    }
}
