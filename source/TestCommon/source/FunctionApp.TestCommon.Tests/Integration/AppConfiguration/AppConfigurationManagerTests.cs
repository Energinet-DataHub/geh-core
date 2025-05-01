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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.AppConfiguration;

public class AppConfigurationManagerTests : IClassFixture<AppConfigurationManagerFixture>
{
    private const string NotExistingFeatureFlag = "geh-core-test-common-not-existing";

    /// <summary>
    /// This feature flag doesn't exists the very first time we run the tests using it,
    /// or if the integration test environment is redeployed.
    /// But after the first time we set its value it will exist, because configuring it will create it.
    /// We could create this feature flag using infrastructure, but this is simpler.
    /// </summary>
    private const string ExistingFeatureFlag = "geh-core-test-common-integrationtests";

    private readonly AppConfigurationManagerFixture _fixture;

    public AppConfigurationManagerTests(AppConfigurationManagerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Given_FeatureFlagDoesNoExist_When_DeleteFeatureFlagAsync_Then_NoExceptionIsThrown()
    {
        // Act
        await _fixture.Sut.DeleteFeatureFlagAsync(NotExistingFeatureFlag);
    }

    [Fact]
    public async Task Given_FeatureFlagDoesNoExist_When_GetFeatureFlagStateAsync_Then_ThrowsExpectedException()
    {
        // Act
        var act = () => _fixture.Sut.GetFeatureFlagStateAsync(NotExistingFeatureFlag);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid feature flag name*");
    }

    [Fact]
    public async Task Given_FeatureFlagExists_When_SetFeatureFlagAsDisabled_Then_FeatureFlagIsDisabled()
    {
        // Act
        await _fixture.Sut.SetFeatureFlagAsync(ExistingFeatureFlag, isEnabled: false);

        // Assert
        var actual = await _fixture.Sut.GetFeatureFlagStateAsync(ExistingFeatureFlag);
        actual.Should().BeFalse();
    }

    [Fact]
    public async Task Given_FeatureFlagExists_When_SetFeatureFlagAsEnabled_Then_FeatureFlagIsEnabled()
    {
        // Act
        await _fixture.Sut.SetFeatureFlagAsync(ExistingFeatureFlag, isEnabled: true);

        // Assert
        var actual = await _fixture.Sut.GetFeatureFlagStateAsync(ExistingFeatureFlag);
        actual.Should().BeTrue();
    }

    [Fact]
    public async Task Given_FeatureFlagDoesNotExist_When_SetFeatureFlagAsDisabled_Then_FeatureFlagIsCreatedAndDisabled()
    {
        var randomFeatureFlag = $"geh-core-test-common-{Guid.NewGuid()}";

        try
        {
            // Act
            await _fixture.Sut.SetFeatureFlagAsync(randomFeatureFlag, isEnabled: false);

            // Assert
            var actual = await _fixture.Sut.GetFeatureFlagStateAsync(randomFeatureFlag);
            actual.Should().BeFalse();
        }
        finally
        {
            await _fixture.Sut.DeleteFeatureFlagAsync(randomFeatureFlag);
        }
    }
}
