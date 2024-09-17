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

using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;

namespace ExampleHost.FunctionApp.Tests.Fixtures;

public static class FunctionHostExtensions
{
    public static bool WasMessageLogged(this FunctionAppHostManager hostManager, string logMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logMessage);

        return hostManager
            .GetHostLogSnapshot()
            .Any(log => log.Contains(logMessage, StringComparison.OrdinalIgnoreCase));
    }
}
