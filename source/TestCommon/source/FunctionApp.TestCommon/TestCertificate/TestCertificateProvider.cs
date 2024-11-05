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
    /// Installs the test-common dev certificate.
    /// Supports silent installation on a GitHub runner if executed as administrator.
    ///
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             When exeuted on a GitHub runner: The certificate will be installed silently if the runner is
    ///             executed as administrator (default).
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             When executed on any non-Github runner: A dialog will be shown to the user the first time the
    ///             certificate is installed. The dialog requests the user to accept trusting the test certificate.
    ///         </description>
    ///     </item>
    /// </list>
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

        using var certificateStore = new X509Store(StoreName.My, storeLocation);
        certificateStore.Open(OpenFlags.ReadWrite);

        using var testCertificate = new X509Certificate2(FilePath, Password);
        certificateStore.Add(testCertificate);
    }
}
