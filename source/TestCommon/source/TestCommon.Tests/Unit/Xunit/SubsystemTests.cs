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
public class SubsystemTests
{
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static bool _subsystemTestShouldBeSkipped = false;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    private static bool _subsystemFactWasSkipped = true;

    public SubsystemTests()
    {
        Environment.SetEnvironmentVariable(
            "SUBSYSTEMFACT_SKIP",
            "false");
        //_subsystemTestShouldBeSkipped.ToString().ToLower());
    }

    [Fact]
    [ScenarioStep(1)]
    public void Given_SubsystemFactIsSkipped()
    {
        _subsystemTestShouldBeSkipped = false;
    }

    [SubsystemFact]
    [ScenarioStep(2)]
    public void When_TestMethodWithSubsystemFact()
    {
        _subsystemFactWasSkipped = false;
    }

    [Fact]
    [ScenarioStep(3)]
    public void Then_TestMethodWithSubsystemFactIsNotExecuted()
    {
        Assert.False(_subsystemFactWasSkipped);
    }
}
