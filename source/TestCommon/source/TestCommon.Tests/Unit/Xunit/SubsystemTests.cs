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
using Energinet.DataHub.Core.TestCommon.Xunit.Configuration;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Energinet.DataHub.Core.TestCommon.Tests.Unit.Xunit;

[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class SubsystemTests
{
    private static bool _subsystemFactWasSkipped = true;

    [Fact]
    [ScenarioStep(1)]
    public void Given_SubsystemFactIsSkipped()
    {
        var configuration = new SubsystemTestConfiguration();
        var shouldSkipSubsystemTests = configuration.Root.GetValue("SUBSYSTEMFACT_SKIP", defaultValue: false);
        Assert.True(shouldSkipSubsystemTests);
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
        Assert.True(_subsystemFactWasSkipped);
    }
}
