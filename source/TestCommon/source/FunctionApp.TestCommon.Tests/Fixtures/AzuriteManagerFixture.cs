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

using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures;

/// <summary>
/// This fixtures ensures we reuse a started <see cref="AzuriteManager"/>
/// and allows us to configure OAuth for the AzuriteManager.
/// </summary>
public sealed class AzuriteManagerFixture : IDisposable
{
    public AzuriteManagerFixture()
    {
    }

    public AzuriteManager? AzuriteManager { get; private set; }

    public void Dispose()
    {
        if (AzuriteManager != null)
        {
            AzuriteManager.Dispose();
        }
    }

    /// <summary>
    /// Ensure we create Azurite according to settings
    /// and only start it the first time we call this method.
    /// </summary>
    /// <param name="useOAuth">If true then start Azurite with OAuth and HTTPS options.</param>
    public void StartAzuriteOnce(bool useOAuth)
    {
        if (AzuriteManager == null)
        {
            AzuriteManager = new AzuriteManager(useOAuth);
            AzuriteManager.StartAzurite();
        }
    }
}
