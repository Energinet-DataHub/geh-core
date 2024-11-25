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
using Energinet.DataHub.Core.FeatureManagement.SampleApp.Common;
using Energinet.DataHub.Core.FeatureManagement.SampleApp.Tests.Fixtures;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Core.FeatureManagement.SampleApp.Tests.Integration.Functions
{
    public class FeatureFlaggedFunctionTests
    {
        private const string MessageRoute = "api/message";

        /// <summary>
        /// Tests where the <see cref="FeatureFlags.Names.UseGuidMessage"/> feature is disabled.
        /// </summary>
        [Collection(nameof(SampleFunctionAppCollectionFixture))]
        public class GetMessageAsync_UseGuidMessageFeatureFlagIsFalse : FunctionAppTestBase<SampleFunctionAppFixture>
        {
            public GetMessageAsync_UseGuidMessageFeatureFlagIsFalse(SampleFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                // Configure the feature flag for the test.
                // The Function App is only restarted if the current state of the feature flag is different from what we need for the test.
                Fixture.HostManager.RestartHostIfChanges(new Dictionary<string, string>
                {
                    { Fixture.UseGuidMessageSettingName, "false" },
                });
            }

            [Fact]
            public async Task When_Requested_Then_AHttp200ResponseIsReturned()
            {
                // Arrange
                using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            [Fact]
            public async Task When_Requested_Then_AStaticMessageTextIsReturned()
            {
                // Arrange
                using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var content = await actualResponse.Content.ReadAsStringAsync();
                content.Should().Be("Static message text");
            }
        }

        /// <summary>
        /// Tests where the <see cref="FeatureFlags.Names.UseGuidMessage"/> feature is enabled.
        /// </summary>
        [Collection(nameof(SampleFunctionAppCollectionFixture))]
        public class GetMessageAsync_UseGuidMessageFeatureFlagIsTrue : FunctionAppTestBase<SampleFunctionAppFixture>
        {
            public GetMessageAsync_UseGuidMessageFeatureFlagIsTrue(SampleFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                // Configure the feature flag for the test.
                // The Function App is only restarted if the current state of the feature flag is different from what we need for the test.
                Fixture.HostManager.RestartHostIfChanges(new Dictionary<string, string>
                {
                    { Fixture.UseGuidMessageSettingName, "true" },
                });
            }

            [Fact]
            public async Task When_Requested_Then_AHttp200ResponseIsReturned()
            {
                // Arrange
                using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            [Fact]
            public async Task When_Requested_Then_AGuidIsReturned()
            {
                // Arrange
                using var request = new HttpRequestMessage(HttpMethod.Get, MessageRoute);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var content = await actualResponse.Content.ReadAsStringAsync();
                Guid.TryParse(content, out _).Should().BeTrue();
            }
        }

        /// <summary>
        /// Tests demonstrating how we can disable a function completely.
        /// </summary>
        [Collection(nameof(SampleFunctionAppCollectionFixture))]
        public class CreateMessage : FunctionAppTestBase<SampleFunctionAppFixture>
        {
            public CreateMessage(SampleFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            [Theory]
            [InlineData("false", HttpStatusCode.Accepted)]
            [InlineData("true", HttpStatusCode.NotFound)]
            public async Task When_RequestedWhenDisabledValueIs_Then_ExpectedStatusCodeIsReturned(string disabledValue, HttpStatusCode expectedStatusCode)
            {
                // Arrange
                // Configure the disabled flag for the test.
                // The Function App is only restarted if the current state of the flag is different from what we need for the test.
                Fixture.HostManager.RestartHostIfChanges(new Dictionary<string, string>
                {
                    { Fixture.CreateMessageDisabledSettingName, disabledValue },
                });

                using var request = new HttpRequestMessage(HttpMethod.Post, MessageRoute);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(expectedStatusCode);
            }
        }
    }
}
