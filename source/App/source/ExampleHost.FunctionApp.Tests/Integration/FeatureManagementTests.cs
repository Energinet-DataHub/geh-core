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

using System.Net;
using Energinet.DataHub.Core.TestCommon;
using ExampleHost.FunctionApp.Tests.Fixtures;
using ExampleHost.FunctionApp01.Common;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.FunctionApp.Tests.Integration;

/// <summary>
/// Tests verifying the configuration and behaviour of Feature Management (feature flags).
/// </summary>
public class FeatureManagementTests
{
    private const string MessageRoute = "api/message";

    /// <summary>
    /// Tests demonstrating use of a local <see cref="FeatureFlags.Names.UseGetMessage"/> feature flag.
    /// </summary>
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class GetMessage_UseGetMessageFeatureFlagIsFalse
    {
        public GetMessage_UseGetMessageFeatureFlagIsFalse(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();
        }

        private ExampleHostsFixture Fixture { get; }

        [Theory]
        [InlineData("false", "Disabled")]
        [InlineData("true", "Enabled")]
        public async Task When_RequestedWhenDisabledValueIs_Then_ExpectedContentIsReturned(string disabledValue, string expectedContent)
        {
            // Arrange
            // Configure the feature flag (locally) for the test.
            // The Function App is only restarted if the current state of the flag is different from what we need for the test.
            Fixture.App01HostManager.RestartHostIfChanges(new Dictionary<string, string>
            {
                { Fixture.UseGetMessageSettingName, disabledValue },
            });

            using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be(expectedContent);
        }
    }

    /// <summary>
    /// Tests demonstrating how we can disable a function completely.
    /// </summary>
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class CreateMessage
    {
        public CreateMessage(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();
        }

        private ExampleHostsFixture Fixture { get; }

        [Theory]
        [InlineData("false", HttpStatusCode.Accepted)]
        [InlineData("true", HttpStatusCode.NotFound)]
        public async Task When_RequestedWhenDisabledValueIs_Then_ExpectedStatusCodeIsReturned(string disabledValue, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            // Configure the disabled flag (locally) for the test.
            // The Function App is only restarted if the current state of the flag is different from what we need for the test.
            Fixture.App01HostManager.RestartHostIfChanges(new Dictionary<string, string>
            {
                { Fixture.CreateMessageDisabledSettingName, disabledValue },
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, MessageRoute);

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(expectedStatusCode);
        }
    }

    /// <summary>
    /// Various tests that proves how Feature Manager can be used together with Azure App Configuration
    /// to refresh feature flags at runtime.
    /// </summary>
    /// <remarks>
    /// Similar tests exists for Web App in the 'FeatureManagementTest' class
    /// located in the 'ExampleHost.WebApi.Tests' project.
    /// </remarks>
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class GetFeatureFlagState_LocalFeatureFlagIsTrue
    {
        private const string LocalFeatureFlag = "local";

        private const string NotExistingFeatureFlag = "geh-core-app-not-existing";

        /// <summary>
        /// This feature flag doesn't exists the very first time we run the tests using it,
        /// or if the integration test environment is redeployed.
        /// But after the first time we set its value it will exist, because configuring it will create it.
        /// We could create this feature flag using infrastructure, but this is simpler.
        /// </summary>
        private const string AzureFeatureFlag = "geh-core-app-integrationtests";

        public GetFeatureFlagState_LocalFeatureFlagIsTrue(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();

            // Configure the feature flag (locally) for the test.
            // The Function App is only restarted if the current state of the feature flag is different from what we need for the test.
            Fixture.App01HostManager.RestartHostIfChanges(new Dictionary<string, string>
            {
                { $"{FeatureFlags.ConfigurationPrefix}{LocalFeatureFlag}", "true" },
            });
        }

        private ExampleHostsFixture Fixture { get; }

        [Fact]
        public async Task When_RequestedForLocalFeatureFlag_Then_FeatureIsEnabled()
        {
            var isEnabled = await RequestFeatureFlagStateAsync(LocalFeatureFlag);
            isEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task When_RequestedForNotExistingFeatureFlag_Then_FeatureIsDisabled()
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
        ///  configBuilder.AddAzureAppConfigurationForIsolatedWorker();
        /// </code>
        ///
        /// 4: Configure AzureAppConfigurationOptions in App Settings or similar.
        /// </summary>
        [Fact]
        public async Task When_ToggleAzureFeatureFlag_Then_FeatureFlagIsRefreshedAndToggled()
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

            wasFeatureFlagToggled.Should().BeTrue("Because we expected the feature flag to be refreshed after 30 seconds.");
        }

        /// <summary>
        /// Call application to use its injected 'IFeatureManager' to get the state of the given feature flag name.
        /// </summary>
        private async Task<bool> RequestFeatureFlagStateAsync(string featureFlagName)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/featureflagstate/{featureFlagName}");
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);
            actualResponse.EnsureSuccessStatusCode();
            var content = await actualResponse.Content.ReadAsStringAsync();

            return content == "Enabled";
        }
    }
}
