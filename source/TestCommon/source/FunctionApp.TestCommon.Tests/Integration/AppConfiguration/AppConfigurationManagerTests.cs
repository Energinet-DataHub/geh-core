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

using Energinet.DataHub.Core.FunctionApp.TestCommon.AppConfiguration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;
using Xunit;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.AppConfiguration;

public class AppConfigurationManagerTests
{
    public AppConfigurationManagerTests()
    {
    }

    [Fact]
    public async Task Given_FeatureFlagDoesNoExist_When_DeleteFeatureFlagAsync_Then_NoExceptionIsThrown()
    {
        var sut = new AppConfigurationManager(
            SingletonIntegrationTestConfiguration.Instance.AppConfigurationEndpoint,
            SingletonIntegrationTestConfiguration.Instance.Credential);

        var missingFeatureFlag = "missing-feature-flag";

        await sut.DeleteFeatureFlagAsync(missingFeatureFlag);
    }

    [Fact]
    public async Task Given_FeatureFlagDoesNotExist_When_SetFeatureFlagAsEnabled_Then_FeatureFlagIsCreatedAndEnabled()
    {
        var sut = new AppConfigurationManager(
            SingletonIntegrationTestConfiguration.Instance.AppConfigurationEndpoint,
            SingletonIntegrationTestConfiguration.Instance.Credential);

        var missingFeatureFlag = "missing-feature-flag";

        await sut.SetFeatureFlagAsync(missingFeatureFlag, isEnabled: true);
    }
}
