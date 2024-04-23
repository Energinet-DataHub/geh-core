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
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.TestCommon.Diagnostics;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// This fixtures ensures we reuse <see cref="ConnectionString"/>
/// so we only have to retrieve an access token and values in Key Vault one time.
///
/// When testing the <see cref="ServiceBusListenerMock"/> we must create new queues/topics
/// per test, because stopping the underlying processors doesn't seem to be 100% deterministics.
/// Also notice the remark on <see cref="ServiceBusListenerMock.ResetMessageReceiversAsync"/>.
/// </summary>
public class ServiceBusListenerMockFixture
{
    public ServiceBusListenerMockFixture()
    {
        TestLogger = new TestDiagnosticsLogger();

        ConnectionString = SingletonIntegrationTestConfiguration.Instance.ServiceBusConnectionString;
    }

    public ITestDiagnosticsLogger TestLogger { get; }

    public string ConnectionString { get; }
}
