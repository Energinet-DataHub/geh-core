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

namespace Energinet.DataHub.Core.App.Common.Identity;

/// <summary>
/// We use this provider to be able to create an implementation of
/// <see cref="TokenCredential"/> without registering the type directly.
/// </summary>
/// <remarks>
/// The actual token credential implementation created will depend on whether
/// the application is running in an Azure App Service or not (e.g. locally or CI/CD runners).
/// </remarks>
public sealed class TokenCredentialProvider
{
    private readonly Lazy<TokenCredential> _credential = new(CreateCredential, isThreadSafe: true);

    public TokenCredential Credential => _credential.Value;

    private static TokenCredential CreateCredential()
    {
        if (IsRunningInAzure())
        {
            return new ManagedIdentityCredential();
        }
        else
        {
            // We expect DefaultAzureCredential uses caching and handles token refresh.
            // However the documentation is a bit unclear: https://learn.microsoft.com/da-dk/dotnet/azure/sdk/authentication/best-practices?tabs=aspdotnet#understand-when-token-lifetime-and-caching-logic-is-needed
            return new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = false,
                ExcludeVisualStudioCredential = false,
                ExcludeAzureCliCredential = false,
                // Here we disable authentication mechanisms that is not used for development
                // See also: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet#defaultazurecredential
                ExcludeWorkloadIdentityCredential = true,
                ExcludeManagedIdentityCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeAzureDeveloperCliCredential = true,
                ExcludeInteractiveBrowserCredential = true,
            });
        }
    }

    private static bool IsRunningInAzure()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
    }
}
