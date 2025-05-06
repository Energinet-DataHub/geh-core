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

using Energinet.DataHub.Core.TestCommon;
using ExampleHost.WebApi.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Tests verifying the configuration and behaviour of Feature Management (feature flags).
/// </summary>
/// <remarks>
/// Similar tests exists for Function App in the 'FeatureManagementTests' class
/// located in the 'ExampleHost.FunctionApp.Tests' project.
/// </remarks>
[Collection(nameof(ExampleHostCollectionFixture))]
public class FeatureManagementTests
{
    public const string LocalFeatureFlag = "local";

    private const string NotExistingFeatureFlag = "geh-core-app-not-existing";

    /// <summary>
    /// This feature flag doesn't exists the very first time we run the tests using it,
    /// or if the integration test environment is redeployed.
    /// But after the first time we set its value it will exist, because configuring it will create it.
    /// We could create this feature flag using infrastructure, but this is simpler.
    /// </summary>
    private const string AzureFeatureFlag = "geh-core-app-integrationtests";

    public FeatureManagementTests(ExampleHostFixture fixture)
    {
        Fixture = fixture;
    }

    private ExampleHostFixture Fixture { get; }

    [Fact]
    public async Task Given_LocalFeatureFlagIsEnabledInAppSettings_When_RequestedForLocalFeatureFlag_Then_FeatureIsEnabled()
    {
        var isEnabled = await RequestFeatureFlagStateAsync(LocalFeatureFlag);
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Given_NotExistingFeatureFlag_When_RequestedForNotExistingFeatureFlag_Then_FeatureIsDisabled()
    {
        var isEnabled = await RequestFeatureFlagStateAsync(NotExistingFeatureFlag);
        isEnabled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that changes to feature flags in Azure App Configuration will be
    /// detected by the host at runtime (refreshed).
    ///
    /// Requirements for this test:
    ///
    /// 1: Host must register services:
    /// <code>
    /// services
    ///     .AddAzureAppConfiguration()
    ///     .AddFeatureManagement();
    /// </code>
    ///
    /// 2: Host must enable middleware:
    /// <code>
    /// builder.UseAzureAppConfiguration();
    /// </code>
    ///
    /// 3: Host must configure Azure App Configuration:
    /// <code>
    ///  builder.Configuration.AddAzureAppConfigurationForWebApp();
    /// </code>
    ///
    /// 4: Configure AzureAppConfigurationOptions in App Settings or similar.
    /// </summary>
    [Fact]
    public async Task Given_AzureFeatureFlagExistsInAzureAppConfiguration_When_ToggleAzureFeatureFlag_Then_FeatureFlagIsRefreshedAndToggled()
    {
        // Assert
        var initialStateInApplication = await RequestFeatureFlagStateAsync(AzureFeatureFlag);

        // Act
        await Fixture.AppConfigurationManager.SetFeatureFlagAsync(AzureFeatureFlag, !initialStateInApplication);

        // Assert
        // => Refresh should happen after 5 seconds (configured in fixture)
        var waitLimit = TimeSpan.FromSeconds(6);
        var delay = TimeSpan.FromSeconds(2);

        var wasFeatureFlagToggled = await Awaiter
            .TryWaitUntilConditionAsync(
                async () =>
                {
                    var currentStateInApplication = await RequestFeatureFlagStateAsync(AzureFeatureFlag);
                    return currentStateInApplication != initialStateInApplication;
                },
                waitLimit,
                delay);

        wasFeatureFlagToggled.Should().BeTrue("Because we expected the feature flag to be refreshed wait limit.");
    }

    /// <summary>
    /// Call application to use its injected 'IFeatureManager' to get the state of the given feature flag name.
    /// </summary>
    private async Task<bool> RequestFeatureFlagStateAsync(string featureFlagName)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi01/featuremanagement/{featureFlagName}");
        var actualResponse = await Fixture.Web01HttpClient.SendAsync(request);
        actualResponse.EnsureSuccessStatusCode();
        var content = await actualResponse.Content.ReadAsStringAsync();

        return content == "Enabled";
    }
}
