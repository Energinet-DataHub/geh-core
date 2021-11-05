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

using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration
{
    /// <summary>
    /// Responsible for extracting secrets necessary for using Azure resources the Integration Test environment.
    ///
    /// On developer machines we use the 'integrationtest.local.settings.json' to set the Key Vault url.
    /// On hosted agents we use an environment variable to set the Key Vault url.
    ///
    /// Developers, and the service principal under which the tests are executed, has access to the Key Vault
    /// and can extract secrets.
    /// </summary>
    public class IntegrationTestConfiguration
    {
        public IntegrationTestConfiguration()
        {
            Configuration = BuildKeyVaultConfigurationRoot();

            ServiceBusConnectionString = Configuration.GetValue("AZURE-SERVICEBUS-CONNECTIONSTRING");
        }

        /// <summary>
        /// Can be used to extract secrets from the Key Vault in the Integration Test environment.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Connection string to the Azure Service Bus in the Integration Test environment.
        /// </summary>
        public string ServiceBusConnectionString { get; }

        private static IConfigurationRoot BuildKeyVaultConfigurationRoot()
        {
            var integrationtestConfiguration = new ConfigurationBuilder()
                .AddJsonFile("integrationtest.local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var keyVaultUrl = integrationtestConfiguration.GetValue("AZURE_KEYVAULT_URL");

            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(keyVaultUrl)
                .Build();
        }
    }
}
