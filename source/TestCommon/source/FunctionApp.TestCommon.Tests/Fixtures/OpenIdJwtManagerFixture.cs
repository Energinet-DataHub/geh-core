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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Integration.OpenIdJwt;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// The fixture used in <see cref="OpenIdJwtManagerTests"/>. Used to ensure that `IntegrationTestConfiguration` is
/// only initialized once.
/// </summary>
public sealed class OpenIdJwtManagerFixture
{
    public AzureB2CSettings AzureB2CSettings =>
        SingletonIntegrationTestConfiguration.Instance.B2CSettings;
}
