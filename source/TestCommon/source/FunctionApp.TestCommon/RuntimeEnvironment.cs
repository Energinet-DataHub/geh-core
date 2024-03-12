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

namespace Energinet.DataHub.Core.FunctionApp.TestCommon;

public class RuntimeEnvironment
{
    public static RuntimeEnvironment Default => new();

    /// <summary>
    /// Represent a connection string to overwrite default behaviour of using LocalDB
    /// </summary>
    public virtual string? TestCommonConnectionString => GetEnvironmentVariable(nameof(TestCommonConnectionString));

    /// <summary>
    /// Get value from host environment
    /// </summary>
    /// <param name="variableName">name of environment variable</param>
    /// <returns>Value of environment variable</returns>
    protected virtual string? GetEnvironmentVariable(string variableName)
        => Environment.GetEnvironmentVariable(variableName);
}
