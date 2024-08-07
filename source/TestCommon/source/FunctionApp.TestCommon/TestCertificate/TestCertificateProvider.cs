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

using System.Security.Cryptography.X509Certificates;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.TestCertificate;

/// <summary>
/// Provides a test certificate which is used to support HTTPS in the <see cref="AzuriteManager"/> and the <see cref="OpenIdJwtManager"/>
/// </summary>
internal static class TestCertificateProvider
{
    /// <summary>
    /// Path to the test certificate file, which is added as content to current NuGet package.
    /// </summary>
    internal const string FilePath = @".\TestCertificate\test-common-cert.pfx";

    /// <summary>
    /// Password to the test certificate file.
    /// </summary>
    internal const string Password = "test-common";

    /// <summary>
    /// Installs test certificate.
    /// Supports silent installation on a GitHub runner if executed as administrator.
    /// </summary>
    internal static void InstallCertificate()
    {
        // If not executed as administrator we can only install to 'CurrentUser' and this will show a dialog to the user.
        var storeLocation = StoreLocation.CurrentUser;

        // Determine if executed on a GitHub runner.
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            // If executed as administrator we can install silently to 'LocalMachine'.
            storeLocation = StoreLocation.LocalMachine;
        }

        using var certificateStore = new X509Store(StoreName.Root, storeLocation);
        certificateStore.Open(OpenFlags.ReadWrite);

        using var testCertificate = new X509Certificate2(FilePath, Password);
        certificateStore.Add(testCertificate);
    }
}
