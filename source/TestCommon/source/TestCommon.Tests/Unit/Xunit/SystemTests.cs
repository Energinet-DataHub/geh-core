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
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Xunit;

namespace Energinet.DataHub.Core.TestCommon.Tests.Unit.Xunit;

[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererValues.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererValues.OrdererAssemblyName)]
public class SystemTests
{
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static bool _systemTestShouldBeSkipped = false;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    private static bool _systemFactWasSkipped = true;

    public SystemTests()
    {
        Environment.SetEnvironmentVariable(
            "SYSTEMFACT_SKIP",
            "false");
        //_systemTestShouldBeSkipped.ToString().ToLower());
    }

    [Fact]
    [ScenarioStep(1)]
    public void Given_SystemFactIsSkipped()
    {
        _systemTestShouldBeSkipped = false;
    }

    [SystemFact]
    [ScenarioStep(2)]
    public void When_TestMethodWithSystemFact()
    {
        _systemFactWasSkipped = false;
    }

    [Fact]
    [ScenarioStep(3)]
    public void Then_TestMethodWithSystemFactIsNotExecuted()
    {
        Assert.False(_systemFactWasSkipped);
    }
}
