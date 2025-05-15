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

using Energinet.DataHub.Core.App.Common.Identity;
using ExampleHost.FunctionApp01.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExampleHost.FunctionApp01.Extensions.DependencyInjection;

internal static class HttpClientExtensions
{
    /// <summary>
    /// Register ExampleHost.FunctionApp02 HTTP client to automatically
    /// retrieve a JWT and add it to the "Authorization" header.
    /// </summary>
    /// <remarks>
    /// Expects <see cref="IAuthorizationHeaderProvider"/> has been registered.
    /// </remarks>
    public static IServiceCollection AddApp02HttpClient(this IServiceCollection services)
    {
        services
            .AddOptions<App02HttpClientsOptions>()
            .BindConfiguration(App02HttpClientsOptions.SectionName)
            .ValidateDataAnnotations();

        services.AddHttpClient(HttpClientNames.App02Api, (sp, httpClient) =>
        {
            var headerProvider = sp.GetRequiredService<IAuthorizationHeaderProvider>();
            var options = sp.GetRequiredService<IOptions<App02HttpClientsOptions>>().Value;

            httpClient.BaseAddress = new Uri(options.ApiBaseAddress);
            httpClient.DefaultRequestHeaders.Authorization = headerProvider.CreateAuthorizationHeader(options.ApplicationIdUri);
        });

        return services;
    }
}
