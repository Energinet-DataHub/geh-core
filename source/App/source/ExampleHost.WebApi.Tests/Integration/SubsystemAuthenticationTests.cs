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

using ExampleHost.WebApi.Tests.Fixtures;
using Xunit;

namespace ExampleHost.WebApi.Tests.Integration;

/// <summary>
/// Subsystem Authentication tests that verifies the subsystem authentication
/// configuration in ExampleHost.WebApi02 is working as expected with
/// the attributes '[AllowAnonymous]' and '[Authorize]'.
/// </summary>
/// <remarks>
/// Similar tests exists for Function App in the 'SubsystemAuthenticationTests' class
/// located in the 'ExampleHost.FunctionApp.Tests' project.
/// </remarks>
[Collection(nameof(ExampleHostCollectionFixture))]
public class SubsystemAuthenticationTests
{
}
