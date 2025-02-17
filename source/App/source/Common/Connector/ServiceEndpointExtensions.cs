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

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Energinet.DataHub.Core.App.Common.Connector;

/// <summary>
/// Provides extension methods for configuring and using service endpoints in an application.
/// </summary>
public static class ServiceEndpointExtensions
{
    /// <summary>
    /// Adds a service endpoint to the service collection using the specified configuration root.
    /// </summary>
    /// <typeparam name="TService">The type of the service endpoint.</typeparam>
    /// <param name="services">The service collection to add the service endpoint to.</param>
    /// <param name="root">The configuration root containing the service endpoint settings.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services or root is null.</exception>
    public static IServiceCollection AddServiceEndpoint<TService>(
        this IServiceCollection services,
        IConfigurationRoot root)
        where TService : ServiceEndpoint
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(root, nameof(root));
        var serviceName = typeof(TService).Name;

        var section = root.GetSection(serviceName);
        return !section.Exists()
            ? throw new InvalidOperationException($"Configuration section '{serviceName}' is missing.")
            : AddServiceEndpoint<TService>(services, section);
    }

    /// <summary>
    /// Adds a service endpoint to the service collection using the specified configuration section.
    /// </summary>
    /// <typeparam name="TService">The type of the service endpoint.</typeparam>
    /// <param name="services">The service collection to add the service endpoint to.</param>
    /// <param name="configurationSection">The configuration containing the service endpoint settings.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services, configuration, or sectionName is null or empty.</exception>
    public static IServiceCollection AddServiceEndpoint<TService>(this IServiceCollection services, IConfigurationSection configurationSection)
        where TService : ServiceEndpoint
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configurationSection, nameof(configurationSection));
        if (!configurationSection.Exists()) throw new InvalidOperationException("Configuration section is missing.");

        var namedHttpClient = GetNamedHttpClient<TService>();
        services.Configure<TService>(configurationSection);

        if (services.All(s => s.ServiceType != typeof(IMemoryCache))) services.AddMemoryCache();

        services.AddTransient<AuthenticateRequestMessageHandler<TService>>();
        services.AddHttpClient(namedHttpClient)
            .ConfigureHttpClient(
                (sp, client) =>
            {
                var service = sp.GetRequiredService<IOptions<TService>>().Value;
                client.BaseAddress = service.BaseAddress;
            })
            .AddHttpMessageHandler<AuthenticateRequestMessageHandler<TService>>()
            .AddPolicyHandler(RetryPolicy());

        services.AddTransient(
            sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var client = httpClientFactory.CreateClient(namedHttpClient);
                return new ServiceEndpoint<TService>(client);
            });

        return services;
    }

    /// <summary>
    /// Gets an HttpClient for the specified service endpoint type.
    /// </summary>
    /// <typeparam name="TService">The type of the service endpoint.</typeparam>
    /// <param name="httpClientFactory">The HttpClient factory to create the HttpClient from.</param>
    /// <returns>An HttpClient for the specified service endpoint type.</returns>
    public static HttpClient GetHttpClient<TService>(this IHttpClientFactory httpClientFactory)
        where TService : ServiceEndpoint
    {
        var namedHttpClient = GetNamedHttpClient<TService>();
        return httpClientFactory.CreateClient(namedHttpClient);
    }

    /// <summary>
    /// Gets the name of the HttpClient for the specified service endpoint type.
    /// </summary>
    /// <typeparam name="TService">The type of the service endpoint.</typeparam>
    /// <returns>The name of the HttpClient for the specified service endpoint type.</returns>
    private static string GetNamedHttpClient<TService>()
        where TService : ServiceEndpoint
        => typeof(ServiceEndpoint<TService>).Name;

    /// <summary>
    /// Creates a retry policy for handling transient HTTP errors.
    /// </summary>
    /// <returns>An asynchronous retry policy for HTTP responses.</returns>
    private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);
    }
}
