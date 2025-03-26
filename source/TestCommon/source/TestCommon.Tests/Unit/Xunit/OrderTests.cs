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
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class OrderTests
{
    private static bool _testCase1WasExecuted;
    private static bool _testCase2WasExecuted;
    private static bool _testCase3WasExecuted;

    // Expected order:
    // TestCase3 (Due to ScenarioStep)
    // TestCase1 (Due to 1 being lower than 2)
    // TestCase2
    [Fact]
    [ScenarioStep(2)]
    public void TestCase2()
    {
        _testCase2WasExecuted = true;
        Assert.True(_testCase1WasExecuted);
        Assert.True(_testCase2WasExecuted);
        Assert.True(_testCase3WasExecuted);
    }

    [Fact]
    [ScenarioStep(2)]
    public void TestCase1()
    {
        _testCase1WasExecuted = true;
        Assert.True(_testCase1WasExecuted);
        Assert.False(_testCase2WasExecuted);
        Assert.True(_testCase3WasExecuted);
    }

    [Fact]
    [ScenarioStep(1)]
    public void TestCase3()
    {
        _testCase3WasExecuted = true;
        Assert.False(_testCase1WasExecuted);
        Assert.False(_testCase2WasExecuted);
        Assert.True(_testCase3WasExecuted);
    }
}
