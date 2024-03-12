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

using System.Text;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;

public static class FunctionAppHostManagerExtensions
{
    /// <summary>
    /// Determine if the <paramref name="functionName"/> was executed by searching the log.
    /// </summary>
    /// <param name="hostManager"></param>
    /// <param name="functionName">For some azure function tool versions, this name must contain 'Functions.' as a prefix for the actual function name.</param>
    /// <returns>True if the log contains any enry indicating the function was executed; otherwise false.</returns>
    public static bool CheckIfFunctionWasExecuted(this FunctionAppHostManager hostManager, string functionName)
    {
        if (hostManager is null)
        {
            throw new ArgumentNullException(nameof(hostManager));
        }

        return hostManager.GetHostLogSnapshot()
            .Any(log => log.Contains($"Executed '{functionName}'", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Manually trigger a function. Especially usefull for testing timer triggered functions.
    /// </summary>
    /// <param name="hostManager"></param>
    /// <param name="functionName"></param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task<HttpResponseMessage> TriggerFunctionAsync(this FunctionAppHostManager hostManager, string functionName)
    {
        if (hostManager is null)
        {
            throw new ArgumentNullException(nameof(hostManager));
        }

        return hostManager.HttpClient.PostAsync(
            new Uri($"/admin/functions/{functionName}", UriKind.Relative),
            new StringContent("{\"input\":\"\"}", Encoding.UTF8, "application/json"));
    }
}
