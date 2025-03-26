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

namespace Energinet.DataHub.Core.TestCommon.Xunit.Orderers;

/// <summary>
/// Values used for the TestCaseOrderer attribute
/// <cref name="OrdererTypeName"/> contains the namespace + name of the orderer
/// <cref name="OrdererAssemblyName"/> contains the name of the assembly containing the orderer
/// If these get out of sync, the tests will not run in the correct order. Since xUnit can not find the orderer
/// </summary>
public static class TestCaseOrdererValues
{
    public const string OrdererTypeName = "Energinet.DataHub.Core.TestCommon.Xunit.Orderers.ScenarioStepOrderer";
    public const string OrdererAssemblyName = "Energinet.DataHub.Core.TestCommon";
}
