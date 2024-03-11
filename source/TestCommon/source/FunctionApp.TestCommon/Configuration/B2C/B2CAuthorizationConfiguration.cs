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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C
{
    /// <summary>
    /// Responsible for extracting secrets for authorization needed for performing tests using B2C access tokens.
    ///
    /// On developer machines we use a 'integrationtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    ///
    /// Developers, and the service principal under which the tests are executed, must have access
    /// to the Key Vault so secrets can be extracted.
    /// </summary>
    public class B2CAuthorizationConfiguration
    {
        public B2CAuthorizationConfiguration(
            string environment,
            IEnumerable<string> clientNames)
        {
            RootConfiguration = BuildKeyVaultConfigurationRoot();
            SecretsConfiguration = BuildSecretsKeyVaultConfiguration(RootConfiguration.GetValue<string>("AZURE_B2CSECRETS_KEYVAULT_URL"));

            Environment = environment;
            ClientApps = CreateClientApps(clientNames);

            TenantId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "tenant-id"));

            var backendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "backend-app-id"));
            BackendApp = new B2CAppSettings(backendAppId);
            BackendOpenIdConfigurationUrl = $"https://login.microsoftonline.com/{TenantId}/v2.0/.well-known/openid-configuration";

            var frontendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "frontend-app-id"));
            FrontendApp = new B2CAppSettings(frontendAppId);
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
        /// URL of the Open ID configuration for the backend (necessary when validating tokens).
        /// </summary>
        public string BackendOpenIdConfigurationUrl { get; }

        /// <summary>
        /// Frontend application ID and scope.
        /// </summary>
        public B2CAppSettings FrontendApp { get; }

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
                .Select(clientName => CreateClientAppSettings(clientName))
                .ToDictionary(o => o.Name, o => o);
        }

        private B2CClientAppSettings CreateClientAppSettings(string clientName)
        {
            return new B2CClientAppSettings
            {
                Name = clientName,
                CredentialSettings = new B2CClientAppCredentialsSettings
                {
                    ClientId = SecretsConfiguration.GetValue<string>(BuildB2CClientSecretName(Environment, clientName, "client-id")),
                    ClientSecret = SecretsConfiguration.GetValue<string>(BuildB2CClientSecretName(Environment, clientName, "client-secret")),
                },
            };
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

        private static IConfigurationRoot BuildKeyVaultConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("integrationtest.local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
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
