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

using Azure.Core;
using Azure.Identity;
using Energinet.DataHub.Core.App.Common.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Energinet.DataHub.Core.App.Common.Extensions.DependencyInjection;

public static class IdentityExtensions
{
    /// <summary>
    /// Add a token credential provider that can be used to retrieve a
    /// shared <see cref="TokenCredential"/> implementation.
    /// </summary>
    /// <remarks>
    /// The actual token credential implementation registered will depend on whether
    /// the application is running in an Azure App Service or not (e.g. locally or CI/CD runners).
    /// </remarks>
    public static IServiceCollection AddTokenCredentialProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<TokenCredentialProvider>(sp =>
        {
            if (IsRunningInAzure())
            {
                return new TokenCredentialProvider(new ManagedIdentityCredential());
            }
            else
            {
                // As we register TokenCredentialProvider as singleton and it has the instance
                // of DefaultAzureCredential, we expect it will use caching and handle token refresh.
                // However the documentation is a bit unclear: https://learn.microsoft.com/da-dk/dotnet/azure/sdk/authentication/best-practices?tabs=aspdotnet#understand-when-token-lifetime-and-caching-logic-is-needed
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeEnvironmentCredential = false,
                    ExcludeVisualStudioCredential = false,
                    ExcludeAzureCliCredential = false,
                    // Here we disable authentication mechanisms that is not used for Integration Test environment
                    // See also: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet#defaultazurecredential
                    ExcludeWorkloadIdentityCredential = true,
                    ExcludeManagedIdentityCredential = true,
                    ExcludeVisualStudioCodeCredential = true,
                    ExcludeAzurePowerShellCredential = true,
                    ExcludeAzureDeveloperCliCredential = true,
                    ExcludeInteractiveBrowserCredential = true,
                });
                return new TokenCredentialProvider(credential);
            }
        });

        return services;
    }

    /// <summary>
    /// Add an authorization header provider that can be used when configuring
    /// http clients involved in subsystem-to-subssytem communication.
    /// </summary>
    /// <remarks>
    /// Expects <see cref="TokenCredentialProvider"/> has been registered.
    /// </remarks>
    public static IServiceCollection AddAuthorizationHeaderProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<IAuthorizationHeaderProvider>(sp =>
        {
            return new AuthorizationHeaderProvider(
                sp.GetRequiredService<TokenCredentialProvider>().Credential);
        });

        return services;
    }

    /// <summary>
    /// Determine if application is running in Azure App Service.
    /// </summary>
    /// <returns><see langword="true"/> if application is running in Azure App Service; otherwise <see langword="false"/>.</returns>
    private static bool IsRunningInAzure()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
    }
}
