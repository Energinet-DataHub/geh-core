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

using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// This fixtures ensures we reuse <see cref="FullyQualifiedNamespace"/> and
/// relevant instances, so we only have to retrieve an access token
/// and values in Key Vault one time.
/// </summary>
public class ServiceBusResourceProviderFixture
{
    public ServiceBusResourceProviderFixture()
    {
        TestLogger = new TestDiagnosticsLogger();
        FullyQualifiedNamespace = SingletonIntegrationTestConfiguration.Instance.ServiceBusFullyQualifiedNamespace;
        AdministrationClient = new ServiceBusAdministrationClient(FullyQualifiedNamespace, new DefaultAzureCredential());
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public string FullyQualifiedNamespace { get; }

    public ServiceBusAdministrationClient AdministrationClient { get; }
}
