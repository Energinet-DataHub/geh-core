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

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Azure.Management.EventHub;
using Microsoft.Azure.Management.EventHub.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.EventHub.ResourceProvider
{
    public class EventHubResourceProviderTests
    {
        public EventHubResourceProviderTests()
        {
        }

        [Fact]
        public async Task When_Xxx_Then_Yyy()
        {
            // Arrange
            var integration = new IntegrationTestConfiguration();

            var tenantId = integration.Configuration.GetValue("AZURE-SHARED-TENANTID");
            var subscriptionId = integration.Configuration.GetValue("AZURE-SHARED-SUBSCRIPTIONID");
            var resourceGroup = integration.Configuration.GetValue("AZURE-SHARED-RESOURCEGROUP");
            var clientId = integration.Configuration.GetValue("AZURE-SHARED-SPNID");
            var clientSecret = integration.Configuration.GetValue("AZURE-SHARED-SPNSECRET");

            var context = new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}");
            var clientCredential = new ClientCredential(clientId, clientSecret);

            var authenticationResult = await context.AcquireTokenAsync("https://management.azure.com/", clientCredential)
                .ConfigureAwait(false);

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken);
            IEventHubManagementClient managementClient = new EventHubManagementClient(tokenCredentials)
            {
                SubscriptionId = subscriptionId,
            };

            // Example connection string: 'Endpoint=sb://xxx.servicebus.windows.net/;'
            var namespaceMatchPattern = @"Endpoint=sb://(.*?).servicebus.windows.net/";
            var match = Regex.Match(integration.EventHubConnectionString, namespaceMatchPattern, RegexOptions.IgnoreCase);
            var eventHubNamespace = match.Groups[1].Value;

            var createEventHubOptions = new Eventhub
            {
                MessageRetentionInDays = 1,
                PartitionCount = 1,
            };

            // Act
            var eventHub = await managementClient.EventHubs.CreateOrUpdateAsync(
                resourceGroup,
                eventHubNamespace,
                "name",
                createEventHubOptions);

            await managementClient.EventHubs.DeleteAsync(
                resourceGroup,
                eventHubNamespace,
                eventHub.Name);

            // Assert
            managementClient.Dispose();
        }
    }
}
