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

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Security.Cryptography.X509Certificates;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite
{
    /// <summary>
    /// Used to start Azurite, which is the storage emulator that replaced Azure Storage Emulator.
    /// Remember to dispose, otherwise the Azurite process wont be stopped.
    ///
    /// If we use 'OAuth' a test certificate will be installed on startup:
    ///  * When exeuted on a GitHub runner: The certificate can be installed silently if the runner is
    ///    executed as administrator (default).
    ///  * When executed on any non-Github runner: A dialog will be shown to the user the first time the
    ///  certificate is installed. The dialog requests the user to accept trusting the test certificate.
    ///
    /// In most cases the AzuriteManager should be used in the FunctionAppFixture:
    /// - Create it in the constructor
    /// - Start it in OnInitializeFunctionAppDependenciesAsync()
    /// - Dispose it in OnDisposeFunctionAppDependenciesAsync()
    /// </summary>
    public class AzuriteManager : IDisposable
    {
        /// <summary>
        /// Path to the test certificate file, which is added as content to current NuGet package.
        /// </summary>
        private const string TestCertificateFilePath = @".\Azurite\TestCertificate\azurite-cert.pfx";

        /// <summary>
        /// Password to the test certificate file.
        /// </summary>
        private const string TestCertificatePassword = "azurite";

        private Process? AzuriteProcess { get; set; }

        /// <summary>
        /// Start Azurite.
        /// </summary>
        /// <param name="useOAuth">If true then start Azurite with OAuth and HTTPS options. When this is enabled then the Uri used by any client must use 'localhost' and not '127.0.0.1'.</param>
        public void StartAzurite(bool useOAuth = false)
        {
            StopAzureStorageEmulator();
            StopHangingAzuriteProcess();
            StartAzuriteProcess(useOAuth);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (AzuriteProcess != null)
            {
                KillProcessAndChildrenRecursively(AzuriteProcess.Id);
                AzuriteProcess = null;
            }
        }

        private static void KillProcessAndChildrenRecursively(int processId)
        {
            var queryChildren = $"Select * From Win32_Process Where ParentProcessID = {processId}";
            using var childProcessManagementObjectSearcher = new ManagementObjectSearcher(queryChildren);
            using var childProcessManagementObjectCollection = childProcessManagementObjectSearcher.Get();

            foreach (var managementObject in childProcessManagementObjectCollection)
            {
                KillProcessAndChildrenRecursively(Convert.ToInt32(managementObject["ProcessID"], CultureInfo.InvariantCulture));
            }

            try
            {
                var process = Process.GetProcessById(processId);
                KillAndDisposeProcess(process);
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        /// <summary>
        /// Azure Storage Emulator is still started/used when starting the function app locally from some IDE's.
        /// </summary>
        private static void StopAzureStorageEmulator()
        {
            var storageEmulatorFilePath = @"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";
            if (File.Exists(storageEmulatorFilePath))
            {
                using var storageEmulatorProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = storageEmulatorFilePath,
                        Arguments = "stop",
                    },
                };

                var success = storageEmulatorProcess.Start();
                if (!success)
                {
                    throw new InvalidOperationException("Error when stopping Azure Storage Emulator.");
                }

                // Azure Storage Emulator is stopped using a process that will exit right away.
                var timeout = TimeSpan.FromMinutes(2);
                var hasExited = storageEmulatorProcess.WaitForExit((int)timeout.TotalMilliseconds);
                if (!hasExited)
                {
                    KillAndDisposeProcess(storageEmulatorProcess);
                    throw new InvalidOperationException($"Azure Storage Emulator did not stop within: '{timeout}'");
                }
            }
        }

        /// <summary>
        /// If test code has been used correctly there should be no hanging Azurite process.
        /// </summary>
        private static void StopHangingAzuriteProcess()
        {
            // The Azurite process is called "node.exe"
            var nodeProcesses = Process.GetProcessesByName("node");
            foreach (var nodeProcess in nodeProcesses)
            {
                var parentProcessId = GetParentProcessId(nodeProcess);
                try
                {
                    var parentProcess = Process.GetProcessById((int)parentProcessId);
                    if (!parentProcess.HasExited && parentProcess.ProcessName == "cmd")
                    {
                        // Warning: We assume that Azurite is the only Nodejs process started with cmd.exe
                        KillAndDisposeProcess(nodeProcess);
                    }
                }
                catch (ArgumentException)
                {
                    // Process.GetProcessById() throws ArgumentException when the process id has been killed
                    // We assume that Azurite is the only Nodejs process where the parent has been killed
                    KillAndDisposeProcess(nodeProcess);
                    break;
                }
            }
        }

        private static void KillAndDisposeProcess(Process process)
        {
            process.Kill();
            process.WaitForExit(milliseconds: 10000);
            process.Dispose();
        }

        private static uint GetParentProcessId(Process process)
        {
            var queryParentId = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}";
            using var parentIdMmanagementObjectSearcher = new ManagementObjectSearcher(queryParentId);
            using var searchResults = parentIdMmanagementObjectSearcher.Get().GetEnumerator();
            searchResults.MoveNext();
            using var queryObj = searchResults.Current;
            return (uint)queryObj["ParentProcessId"];
        }

        private void StartAzuriteProcess(bool useOAuth)
        {
            var azuriteBlobCommandFilePath = GetAzuriteBlobCommandFilePath();
            var azuriteArguments = GetAzuriteArguments(useOAuth);

            HandleTestCertificateInstallation(useOAuth);

            AzuriteProcess = new Process
            {
                StartInfo =
                {
                    FileName = azuriteBlobCommandFilePath,
                    Arguments = azuriteArguments,
                    RedirectStandardError = true,
                },
            };
            try
            {
                var success = AzuriteProcess.Start();
                if (!success)
                {
                    throw new InvalidOperationException("Azurite failed to start");
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Remember to install Azurite.\nAzurite failed to start: '{e.Message}'");
            }

            var hasExited = AzuriteProcess.WaitForExit(1000);
            if (hasExited)
            {
                var azuriteError = AzuriteProcess.StandardError.ReadToEnd();
                var errorMessage =
                    $"Azurite failed to start: '{azuriteError}'." +
                    $"\nEnsure tests that are using Azurite are not running in parallel (use ICollectionFixture<TestFixture>)." +
                    $"\nIf another process is using port 10000 then close that application." +
                    $"\nUse 'Get-Process -Id (Get-NetTCPConnection -LocalPort 10000).OwningProcess' to find the other process.";
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// If Azurite is installed globally (-g) a folder path is not needed.
        /// </summary>
        private static string GetAzuriteBlobCommandFilePath()
        {
            var azuriteBlobFileName = "azurite-blob.cmd";
            var azuriteBlobFolderPath = Environment.GetEnvironmentVariable("AzuriteBlobFolderPath");

            return azuriteBlobFolderPath == null
                ? azuriteBlobFileName
                : Path.Combine(azuriteBlobFolderPath, azuriteBlobFileName);
        }

        private static string GetAzuriteArguments(bool useOAuth)
        {
            return useOAuth == true
                ? $"--oauth basic --cert {TestCertificateFilePath} --pwd {TestCertificatePassword}"
                : string.Empty;
        }

        /// <summary>
        /// If using OAuth then installs test certificate.
        /// Supports silent installation on a GitHub runner if executed as administrator.
        /// </summary>
        private static void HandleTestCertificateInstallation(bool useOAuth)
        {
            if (useOAuth)
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

                using var testCertificate = new X509Certificate2(TestCertificateFilePath, TestCertificatePassword);
                certificateStore.Add(testCertificate);
            }
        }
    }
}
