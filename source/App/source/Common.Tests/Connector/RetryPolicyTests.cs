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

using System.Net;
using Energinet.DataHub.Core.App.Common.Connector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Connector;

public class RetryPolicyTests
{
    [Fact]
    public async Task SendAsync_WhenCalled_ShouldRetryOnTransientErrors()
    {
        const int expectedRequestCount = 6; // 1 initial request + 5 retries
        var configuration = CreateConfiguration();

        var mockHttp = new MockHttpMessageHandler();
        var mockedRequest = mockHttp.When("http://localhost:8080/data").Respond(HttpStatusCode.ServiceUnavailable);

        var services = new ServiceCollection();
        services.AddServiceEndpoint<MyServiceEndpoint>(
            configuration.GetSection("MyServiceEndpoint"),
            options => options.ConfigurePrimaryHttpMessageHandler(() => mockHttp));

        var serviceProvider = services.BuildServiceProvider();
        var serviceEndpoint = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/data");

        using var client = serviceEndpoint.GetHttpClient<MyServiceEndpoint>();
        await client.SendAsync(request);

        var matchCount = mockHttp.GetMatchCount(mockedRequest);
        Assert.Equal(expectedRequestCount, matchCount);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(GetServiceEndpointConfiguration("MyServiceEndpoint"))
            .Build();
    }

    private static IEnumerable<KeyValuePair<string, string?>> GetServiceEndpointConfiguration(string sectionName = "ServiceEndpoint")
    {
        yield return new KeyValuePair<string, string?>($"{sectionName}:BaseAddress", "http://localhost:8080");
    }
}
