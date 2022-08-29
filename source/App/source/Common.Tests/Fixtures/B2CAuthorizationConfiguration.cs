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

using System;
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Core.App.Common.Tests.Fixtures
{
    /// <summary>
    /// Responsible for extracting secrets for authorization needed for performing tests using B2C access tokens.
    ///
    /// On developer machines we use a '*.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    ///
    /// Developers, and the service principal under which the tests are executed, must have access
    /// to the Key Vault so secrets can be extracted.
    /// </summary>
    public class B2CAuthorizationConfiguration
    {
        public B2CAuthorizationConfiguration(
            bool usedForSystemTests,
            string environment,
            IEnumerable<string> clientNames)
        {
            var localSettingsJsonFilename = usedFromSystemTests
                ? "systemtest.local.settings.json"
                : "integrationtest.local.settings.json";
            var azureSecretsKeyVaultUrlKey = usedFromSystemTests
                ? "AZURE_SYSTEMTESTS_KEYVAULT_URL"
                : "AZURE_SECRETS_KEYVAULT_URL";
            RootConfiguration = BuildKeyVaultConfigurationRoot(localSettingsJsonFilename);
            SecretsConfiguration = BuildSecretsKeyVaultConfiguration(RootConfiguration.GetValue<string>(azureSecretsKeyVaultUrlKey));

            Environment = environment;
            ClientApps = CreateClientApps(clientNames);

            TenantId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "tenant-id"));

            var backendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "backend-app-id"));
            BackendApp = new B2CAppSettings(backendAppId);

            var frontendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "frontend-app-id"));
            FrontendApp = new B2CAppSettings(frontendAppId);

            ApiManagementBaseAddress = SecretsConfiguration.GetValue<Uri>(BuildApiManagementEnvironmentSecretName(Environment, "host-url"));
            FrontendOpenIdUrl = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "frontend-open-id-url"));
        }

        /// <summary>
        /// Environment short name with instance indication.
        /// </summary>
        public string Environment { get; }

        /// <summary>
        /// Client apps settings.
        /// </summary>
        public IReadOnlyDictionary<string, B2CClientAppSettings> ClientApps { get; }

        /// <summary>
        /// The B2C tenant id in the configured environment.
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// Backend application ID and scope.
        /// </summary>
        public B2CAppSettings BackendApp { get; }

        /// <summary>
        /// Frontend application ID and scope.
        /// </summary>
        public B2CAppSettings FrontendApp { get; }

        /// <summary>
        /// URL of the Open ID configuration for the frontend.
        /// </summary>
        public string FrontendOpenIdUrl { get; }

        /// <summary>
        /// The base address for the API Management in the configured environment.
        /// </summary>
        public Uri ApiManagementBaseAddress { get; }

        private IConfigurationRoot RootConfiguration { get; }

        /// <summary>
        /// Can be used to extract secrets from the Key Vault.
        /// </summary>
        private IConfigurationRoot SecretsConfiguration { get; }

        /// <summary>
        /// Create a dictionary of B2C client apps used for testing, each with its own settings necessary to acquire an access token in a configured environment.
        /// </summary>
        /// <param name="clientNames">List of client names or shorthands. For many clients the name is a team name.</param>
        /// <returns>A dictionary of B2C clients apps used for testing.</returns>
        private IReadOnlyDictionary<string, B2CClientAppSettings> CreateClientApps(IEnumerable<string> clientNames)
        {
            return clientNames
                .Select(clientName => new B2CClientAppSettings(
                        clientName,
                        new B2CClientAppCredentialsSettings(
                            SecretsConfiguration.GetValue<string>(BuildB2CClientSecretName(Environment, clientName, "client-id")),
                            SecretsConfiguration.GetValue<string>(BuildB2CClientSecretName(Environment, clientName, "client-secret")))))
                .ToDictionary(o => o.Name, o => o);
        }

        /// <summary>
        /// Load settings from key vault.
        /// </summary>
        private static IConfigurationRoot BuildSecretsKeyVaultConfiguration(string keyVaultUrl)
        {
            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(keyVaultUrl)
                .Build();
        }

        private static IConfigurationRoot BuildKeyVaultConfigurationRoot(string localSettingsJsonFilename)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(localSettingsJsonFilename, optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static string BuildApiManagementEnvironmentSecretName(string environment, string secret)
        {
            return $"APIM-{environment}-{secret}";
        }

        private static string BuildB2CClientSecretName(string environment, string client, string secret)
        {
            return $"B2C-{environment}-{client}-{secret}";
        }

        private static string BuildB2CEnvironmentSecretName(string environment, string secret)
        {
            return $"B2C-{environment}-{secret}";
        }
    }
}
