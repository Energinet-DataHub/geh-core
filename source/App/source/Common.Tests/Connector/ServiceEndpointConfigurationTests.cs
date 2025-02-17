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

using Energinet.DataHub.Core.App.Common.Connector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Energinet.DataHub.Core.App.Common.Tests.Connector;

public class ServiceEndpointConfigurationTests
{
    [Fact]
    public void ShouldConfigureServiceEndpointOptionsCorrectly()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(GetServiceEndpointConfiguration("MyServiceEndpoint"))
            .Build();

        var services = new ServiceCollection();
        services.AddServiceEndpoint<MyServiceEndpoint>(configuration.GetSection("MyServiceEndpoint"));

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<MyServiceEndpoint>>().Value;

        Assert.Equal(new Uri("https://localhost"), options.BaseAddress);
        Assert.NotNull(options.Identity);
        Assert.Equal("app1", options.Identity.ApplicationIds[0]);
    }

    [Fact]
    public void ShouldThrowInvalidOperationExceptionWhenConfigurationSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(GetServiceEndpointConfiguration("MyServiceEndpoint"))
            .Build();

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddServiceEndpoint<MyServiceEndpoint>(configuration.GetSection("MissingSection")));
    }

    [Fact]
    public void ShouldReturnConfiguredHttpClientFromHttpClientFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(GetServiceEndpointConfiguration("MyServiceEndpoint"))
            .Build();

        var services = new ServiceCollection();
        services.AddServiceEndpoint<MyServiceEndpoint>(configuration.GetSection("MyServiceEndpoint"));
        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.GetHttpClient<MyServiceEndpoint>();

        Assert.NotNull(client);
        Assert.Equal(new Uri("https://localhost"), client.BaseAddress);
    }

    private static IEnumerable<KeyValuePair<string, string?>> GetServiceEndpointConfiguration(string sectionName = "ServiceEndpoint")
    {
        yield return new KeyValuePair<string, string?>($"{sectionName}:BaseAddress", "https://localhost");
        yield return new KeyValuePair<string, string?>($"{sectionName}:Identity:ApplicationIds:0", "app1");
        yield return new KeyValuePair<string, string?>($"{sectionName}:Identity:ApplicationIds:1", "app2");
    }
}
