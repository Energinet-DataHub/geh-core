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

using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;

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
    public IntegrationTestConfiguration(DefaultAzureCredential? defaultAzureCredential = null)
    {
        Configuration = BuildKeyVaultConfigurationRoot(defaultAzureCredential);

        ApplicationInsightsConnectionString = Configuration.GetValue("AZURE-APPINSIGHTS-CONNECTIONSTRING");
        LogAnalyticsWorkspaceId = Configuration.GetValue("AZURE-LOGANALYTICS-WORKSPACE-ID");

        EventHubNamespaceName = Configuration.GetValue("AZURE-EVENTHUB-NAMESPACE");
        EventHubFullyQualifiedNamespace = $"{EventHubNamespaceName}.servicebus.windows.net";
        ServiceBusFullyQualifiedNamespace = $"{Configuration.GetValue("AZURE-SERVICEBUS-NAMESPACE")}.servicebus.windows.net";

        ResourceManagementSettings = CreateResourceManagementSettings(Configuration);
        B2CSettings = CreateB2CSettings(Configuration);
        DatabricksSettings = CreateDatabricksSettings(Configuration);
    }

    /// <summary>
    /// Can be used to extract secrets from the Key Vault in the Integration Test environment.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// Connection string to the Application Insights in the Integration Test environment.
    /// </summary>
    public string ApplicationInsightsConnectionString { get; }

    /// <summary>
    /// ID of the Log Analytics Workspace in the Integration Test environment.
    /// </summary>
    public string LogAnalyticsWorkspaceId { get; }

    /// <summary>
    /// Namespace name of the Azure Event Hub in the Integration Test environment.
    /// </summary>
    public string EventHubNamespaceName { get; }

    /// <summary>
    /// Fully qualified namespace of the Azure Event Hub in the Integration Test environment.
    /// </summary>
    public string EventHubFullyQualifiedNamespace { get; }

    /// <summary>
    /// Fully qualified namespace of the Azure Service Bus in the Integration Test environment.
    /// </summary>
    public string ServiceBusFullyQualifiedNamespace { get; }

    /// <summary>
    /// Settings necessary for managing some Azure resources, like Event Hub, in the Integration Test environment.
    /// </summary>
    public AzureResourceManagementSettings ResourceManagementSettings { get; }

    /// <summary>
    /// Settings necessary for managing the Azure AD B2C.
    /// </summary>
    public AzureB2CSettings B2CSettings { get; }

    /// <summary>
    /// Settings necessary for using the Databricks workspace and SQL Warehouse.
    /// </summary>
    public DatabricksSettings DatabricksSettings { get; }

    private static IConfigurationRoot BuildKeyVaultConfigurationRoot(DefaultAzureCredential? defaultAzureCredential)
    {
        var integrationtestConfiguration = new ConfigurationBuilder()
            .AddJsonFile("integrationtest.local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var keyVaultUrl = integrationtestConfiguration.GetValue("AZURE_KEYVAULT_URL");
        GuardKeyVaultUrl(keyVaultUrl);

        return new ConfigurationBuilder()
            .AddAuthenticatedAzureKeyVault(keyVaultUrl, defaultAzureCredential)
            .Build();
    }

    private static void GuardKeyVaultUrl(string keyVaultUrl)
    {
        if (string.IsNullOrWhiteSpace(keyVaultUrl))
        {
            throw new InvalidOperationException("The 'AZURE_KEYVAULT_URL' is not set. If tests are excuted locally it should be configured in the 'integrationtest.local.settings.json'; otherwise it should be set as an environment variable.");
        }
    }

    private static AzureResourceManagementSettings CreateResourceManagementSettings(IConfigurationRoot configuration)
    {
        return new AzureResourceManagementSettings
        {
            TenantId = configuration.GetValue("AZURE-SHARED-TENANTID"),
            SubscriptionId = configuration.GetValue("AZURE-SHARED-SUBSCRIPTIONID"),
            ResourceGroup = configuration.GetValue("AZURE-SHARED-RESOURCEGROUP"),
            ClientId = configuration.GetValue("AZURE-SHARED-SPNID"),
            ClientSecret = configuration.GetValue("AZURE-SHARED-SPNSECRET"),
        };
    }

    private static AzureB2CSettings CreateB2CSettings(IConfigurationRoot configuration)
    {
        return new AzureB2CSettings
        {
            Tenant = configuration.GetValue("AZURE-B2C-TENANT"),
            ServicePrincipalId = configuration.GetValue("AZURE-B2C-SPN-ID"),
            ServicePrincipalSecret = configuration.GetValue("AZURE-B2C-SPN-SECRET"),
            BackendAppId = configuration.GetValue("AZURE-B2C-TESTBACKEND-APP-ID"),
            BackendServicePrincipalObjectId = configuration.GetValue("AZURE-B2C-TESTBACKEND-SPN-OBJECTID"),
            BackendAppObjectId = configuration.GetValue("AZURE-B2C-TESTBACKEND-APP-OBJECTID"),
            TestBffAppId = configuration.GetValue("AZURE-B2C-TESTBFF-APP-ID"),
        };
    }

    private DatabricksSettings CreateDatabricksSettings(IConfigurationRoot configuration)
    {
        return new DatabricksSettings
        {
            // We have to build the URL here in code as currently this is also what happens in the infrastructure code (terraform).
            WorkspaceUrl = $"https://{configuration.GetValue("dbw-domain-test-workspace-url")}",
            WorkspaceAccessToken = configuration.GetValue("dbw-domain-test-workspace-token"),
            WarehouseId = configuration.GetValue("dbw-sql-endpoint-id"),
        };
    }
}
