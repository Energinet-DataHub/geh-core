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

using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Xunit;

namespace Energinet.DataHub.Core.TestCommon.Tests.Unit.Xunit;

[TestCaseOrderer(
    ordererTypeName: "Energinet.DataHub.Core.TestCommon.Xunit.Orderers.ScenarioStepOrderer",
    ordererAssemblyName: "Energinet.DataHub.Core.TestCommon")]
public class SkippingSystemAndSubsystemTests
{
    private static bool _subSystemTestWasExecuted;
    private static bool _systemTestWasExecuted;

    public SkippingSystemAndSubsystemTests()
    {
        Environment.SetEnvironmentVariable("SUBSYSTEMFACT_SKIP", "true");
        Environment.SetEnvironmentVariable("SYSTEMFACT_SKIP", "true");
    }

    [SubsystemFact]
    [ScenarioStep(1)]
    public void When_SubSystemTestAreSkipped_ThisTestIsSkipped()
    {
        _subSystemTestWasExecuted = true;
    }

    [SystemFact]
    [ScenarioStep(1)]
    public void When_SystemTestAreSkipped_ThisTestIsSkipped()
    {
        _systemTestWasExecuted = true;
    }

    [Fact]
    [ScenarioStep(2)]
    public void When_SubsystemTestsAreSkippedInConfiguration_Then_AllSubSystemTestAreSkipped()
    {
        Assert.False(_subSystemTestWasExecuted);
    }

    [Fact]
    [ScenarioStep(2)]
    public void When_SystemTestsAreSkippedInConfiguration_Then_AllSystemTestAreSkipped()
    {
        Assert.False(_systemTestWasExecuted);
    }
}
