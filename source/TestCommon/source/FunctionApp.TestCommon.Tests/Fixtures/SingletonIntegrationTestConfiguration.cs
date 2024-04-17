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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// Retrieving all key vault values in <see cref="IntegrationTestConfiguration"/>
/// can cost several seconds and waste time in integration tests. However, it is
/// not a good idea to always use it as a singleton, hence we implement this
/// class to be able to use it as a singleton when we believe it makes sense.
/// </summary>
internal sealed class SingletonIntegrationTestConfiguration
{
    static SingletonIntegrationTestConfiguration() { }

    private SingletonIntegrationTestConfiguration() { }

    public static IntegrationTestConfiguration Instance { get; } = new IntegrationTestConfiguration();
}
