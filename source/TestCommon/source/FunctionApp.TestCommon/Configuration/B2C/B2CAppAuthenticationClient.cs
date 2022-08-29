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
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C
{
    /// <summary>
    /// A component that encapsulates the retrieval of an access token for a
    /// B2C client application accessing another B2C application.
    /// </summary>
    public class B2CAppAuthenticationClient
    {
        public B2CAppAuthenticationClient(
            string tenantId,
            B2CAppSettings appSettings,
            B2CClientAppSettings clientAppSettings)
        {
            TenantId = tenantId;
            AppSettings = appSettings;
            ClientAppSettings = clientAppSettings;

            ConfidentialClientApp = CreateConfidentialClientApp(TenantId, ClientAppSettings);
        }

        /// <summary>
        /// Tenant id.
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// The application to which we want an access token.
        /// </summary>
        public B2CAppSettings AppSettings { get; }

        /// <summary>
        /// The client application for which we want an access token.
        /// </summary>
        public B2CClientAppSettings ClientAppSettings { get; }

        private IConfidentialClientApplication ConfidentialClientApp { get; }

        public async Task<AuthenticationResult> GetAuthenticationTokenAsync()
        {
            var result = await ConfidentialClientApp.AcquireTokenForClient(AppSettings.AppScope)
                .ExecuteAsync().ConfigureAwait(false);

            return result;
        }

        private static IConfidentialClientApplication CreateConfidentialClientApp(string tenantId, B2CClientAppSettings clientAppSettings)
        {
            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(clientAppSettings.CredentialSettings.ClientId)
                .WithClientSecret(clientAppSettings.CredentialSettings.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            return confidentialClientApp;
        }
    }
}
