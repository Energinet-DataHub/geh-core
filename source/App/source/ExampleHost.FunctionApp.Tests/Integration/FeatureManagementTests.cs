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
    /// Tests where the <see cref="FeatureFlags.Names.UseGetMessage"/> feature is disabled.
    /// </summary>
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class GetMessage_UseGetMessageFeatureFlagIsFalse
    {
        public GetMessage_UseGetMessageFeatureFlagIsFalse(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();

            // Configure the feature flag (locally) for the test.
            // The Function App is only restarted if the current state of the feature flag is different from what we need for the test.
            Fixture.App01HostManager.RestartHostIfChanges(new Dictionary<string, string>
            {
                { Fixture.UseGetMessageSettingName, "false" },
            });
        }

        private ExampleHostsFixture Fixture { get; }

        [Fact]
        public async Task When_Requested_Then_AHttp200ResponseIsReturned()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_Requested_Then_DisabledTextIsReturned()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Disabled");
        }
    }

    /// <summary>
    /// Tests where the <see cref="FeatureFlags.Names.UseGetMessage"/> feature is enabled.
    /// </summary>
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class GetMessage_UseGetMessageFeatureFlagIsTrue
    {
        public GetMessage_UseGetMessageFeatureFlagIsTrue(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();

            // Configure the feature flag (locally) for the test.
            // The Function App is only restarted if the current state of the feature flag is different from what we need for the test.
            Fixture.App01HostManager.RestartHostIfChanges(new Dictionary<string, string>
            {
                { Fixture.UseGetMessageSettingName, "true" },
            });
        }

        private ExampleHostsFixture Fixture { get; }

        [Fact]
        public async Task When_Requested_Then_AHttp200ResponseIsReturned()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_Requested_Then_EnabledTextIsReturned()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Enabled");
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
    [Collection(nameof(ExampleHostsCollectionFixture))]
    public class GetFeatureFlagState_LocalFeatureFlagIsTrue
    {
        public GetFeatureFlagState_LocalFeatureFlagIsTrue(ExampleHostsFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.SetTestOutputHelper(testOutputHelper);

            Fixture.App01HostManager.ClearHostLog();

            // Configure the feature flag (locally) for the test.
            // The Function App is only restarted if the current state of the feature flag is different from what we need for the test.
            Fixture.App01HostManager.RestartHostIfChanges(new Dictionary<string, string>
            {
                { $"{FeatureFlags.ConfigurationPrefix}Local", "true" },
            });
        }

        private ExampleHostsFixture Fixture { get; }

        [Fact]
        public async Task When_RequestedForLocalFeatureFlag_Then_EnabledTextIsReturned()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/featureflagstate/Local");

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Enabled");
        }

        [Fact]
        public async Task When_RequestedForAzureFeatureFlag_Then_EnabledTextIsReturned()
        {
            // Arrange
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/featureflagstate/Azure");

            // Act
            var actualResponse = await Fixture.App01HostManager.HttpClient.SendAsync(request);

            // Assert
            var content = await actualResponse.Content.ReadAsStringAsync();
            content.Should().Be("Enabled");
        }
    }
}
