﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Diagnostics;
using System.Globalization;
using System.Management;
using Energinet.DataHub.Core.FunctionApp.TestCommon.TestCertificate;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;

/// <summary>
/// Used to start Azurite, which is the storage emulator that replaced Azure Storage Emulator.
/// Remember to dispose, otherwise the Azurite process wont be stopped.
///
/// If 'OAuth' is enabled then a test certificate will be installed on startup (see <see cref="TestCertificateProvider"/>.<see cref="TestCertificateProvider.InstallCertificate"/>)
///
/// In most cases the AzuriteManager should be used in the FunctionAppFixture:
/// <list type="bullet">
///     <item>
///         <description>Create it in the constructor</description>
///     </item>
///     <item>
///         <description>Start it in OnInitializeFunctionAppDependenciesAsync()</description>
///     </item>
///     <item>
///         <description>Dispose it in OnDisposeFunctionAppDependenciesAsync()</description>
///     </item>
/// </list>
///
/// If Azurite is not installed globally then set the environment variable 'AzuriteFolderPath'
/// to the location of the 'azurite.cmd' file.
/// </summary>
public class AzuriteManager : IDisposable
{
    // Azurite accepts the well-known storage account name and key.
    // We can use them to create a valid connection string.
    // See: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&bc=%2Fazure%2Fstorage%2Fblobs%2Fbreadcrumb%2Ftoc.json&tabs=visual-studio#well-known-storage-account-and-key
    private const string WellKnownStorageAccountName = "devstoreaccount1";
    private const string WellKnownStorageAccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    private const string ConnectionStringCommonPart = $"DefaultEndpointsProtocol=https;AccountName={WellKnownStorageAccountName};AccountKey={WellKnownStorageAccountKey};";

    private const int BlobServicePort = 10000;
    private const int QueueServicePort = 10001;
    private const int TableServicePort = 10002;

    /// <summary>
    /// Create manager to startup Azurite.
    /// </summary>
    public AzuriteManager(bool useOAuth = false, bool useSilentMode = true)
    {
        UseOAuth = useOAuth;
        UseSilentMode = useSilentMode;
        FullConnectionString = BuildConnectionString(UseOAuth, useBlob: true, useQueue: true, useTable: true);
        BlobStorageConnectionString = BuildConnectionString(UseOAuth, useBlob: true);
        BlobStorageServiceUri = BuildServiceUri(UseOAuth, BlobServicePort);
        QueueStorageConnectionString = BuildConnectionString(UseOAuth, useQueue: true);
        QueueStorageServiceUri = BuildServiceUri(UseOAuth, QueueServicePort);
        TableStorageConnectionString = BuildConnectionString(UseOAuth, useTable: true);
        TableStorageServiceUri = BuildServiceUri(UseOAuth, TableServicePort);
    }

    /// <summary>
    /// If true then start Azurite with OAuth and HTTPS options.
    /// </summary>
    public bool UseOAuth { get; }

    /// <summary>
    /// If true then start Azurite silent mode (disable output of the access log).
    /// </summary>
    public bool UseSilentMode { get; }

    /// <summary>
    /// Connection string for connecting to all Azurite services (blob, queue, table).
    /// </summary>
    public string FullConnectionString { get; }

    /// <summary>
    /// Connection string for connecting to Azurite blob service only.
    /// </summary>
    public string BlobStorageConnectionString { get; }

    /// <summary>
    /// Service uri to blob storage when connected to Azurite.
    /// </summary>
    public Uri BlobStorageServiceUri { get; }

    /// <summary>
    /// Connection string for connecting to Azurite queue service only.
    /// </summary>
    public string QueueStorageConnectionString { get; }

    /// <summary>
    /// Service uri to queue storage when connected to Azurite.
    /// </summary>
    public Uri QueueStorageServiceUri { get; }

    /// <summary>
    /// Connection string for connecting to Azurite table service only.
    /// </summary>
    public string TableStorageConnectionString { get; }

    /// <summary>
    /// Service uri to table storage when connected to Azurite.
    /// </summary>
    public Uri TableStorageServiceUri { get; }

    private Process? AzuriteProcess { get; set; }

    /// <summary>
    /// Start Azurite.
    /// </summary>
    public void StartAzurite()
    {
        StopAzureStorageEmulator();
        StopHangingAzuriteProcess();
        StartAzuriteProcess();
    }

    /// <summary>
    /// Cleanup Azurite storage.
    /// </summary>
    public void CleanupAzuriteStorage()
    {
        if (Directory.Exists("__blobstorage__"))
            Directory.Delete("__blobstorage__", true);

        if (Directory.Exists("__queuestorage__"))
            Directory.Delete("__queuestorage__", true);

        if (Directory.Exists("__tablestorage__"))
            Directory.Delete("__tablestorage__", true);

        if (File.Exists("__azurite_db_blob__.json"))
            File.Delete("__azurite_db_blob__.json");

        if (File.Exists("__azurite_db_blob_extent__.json"))
            File.Delete("__azurite_db_blob_extent__.json");

        if (File.Exists("__azurite_db_queue__.json"))
            File.Delete("__azurite_db_queue__.json");

        if (File.Exists("__azurite_db_queue_extent__.json"))
            File.Delete("__azurite_db_queue_extent__.json");

        if (File.Exists("__azurite_db_table__.json"))
            File.Delete("__azurite_db_table__.json");

        if (File.Exists("__azurite_db_table_extent__.json"))
            File.Delete("__azurite_db_table_extent__.json");
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

    private static Uri BuildServiceUri(bool useOAuth, int servicePort)
    {
        if (useOAuth)
        {
            // When using OAuth we must use HTTPS and 'localhost' instead of '127.0.0.1'.
            return new Uri($"https://localhost:{servicePort}/{WellKnownStorageAccountName}");
        }

        return new Uri($"http://127.0.0.1:{servicePort}/{WellKnownStorageAccountName}");
    }

    private static string BuildConnectionString(bool useOAuth, bool useBlob = false, bool useQueue = false, bool useTable = false)
    {
        if (useOAuth)
        {
            // When using OAuth we must use HTTPS and 'localhost' instead of '127.0.0.1'.
            return ConnectionStringCommonPart +
                (useBlob ? $"BlobEndpoint=https://localhost:{BlobServicePort}/{WellKnownStorageAccountName};" : string.Empty) +
                (useQueue ? $"QueueEndpoint=https://localhost:{QueueServicePort}/{WellKnownStorageAccountName};" : string.Empty) +
                (useTable ? $"TableEndpoint=https://localhost:{TableServicePort}/{WellKnownStorageAccountName};" : string.Empty);
        }

        return "UseDevelopmentStorage=true";
    }

#pragma warning disable CA1416 // Validate platform compatibility: The following only supported in WIndows
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
#pragma warning restore CA1416 // Validate platform compatibility

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

#pragma warning disable CA1416 // Validate platform compatibility: The following only supported in WIndows
    private static uint GetParentProcessId(Process process)
    {
        var queryParentId = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}";
        using var parentIdMmanagementObjectSearcher = new ManagementObjectSearcher(queryParentId);
        using var searchResults = parentIdMmanagementObjectSearcher.Get().GetEnumerator();
        searchResults.MoveNext();
        using var queryObj = searchResults.Current;
        return (uint)queryObj["ParentProcessId"];
    }
#pragma warning restore CA1416 // Validate platform compatibility

    private void StartAzuriteProcess()
    {
        var azuriteCommandFilePath = GetAzuriteCommandFilePath();
        var azuriteArguments = GetAzuriteArguments(UseOAuth, UseSilentMode);

        if (UseOAuth)
            TestCertificateProvider.InstallCertificate();

        AzuriteProcess = new Process
        {
            StartInfo =
            {
                FileName = azuriteCommandFilePath,
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

        var hasExited = AzuriteProcess.WaitForExit(1500);
        if (hasExited)
        {
            var azuriteError = AzuriteProcess.StandardError.ReadToEnd();
            var errorMessage =
                $"Azurite failed to start: '{azuriteError}'." +
                $"\nEnsure tests that are using Azurite are not running in parallel (use ICollectionFixture<TestFixture>)." +
                $"\nIf another process is using ports 10000, 10001 or 10002 then close that application." +
                $"\nE.g. use 'Get-Process -Id (Get-NetTCPConnection -LocalPort 10000).OwningProcess' to find a process that uses port 10000.";
            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// If Azurite is installed globally (-g) a folder path is not needed.
    /// </summary>
    private static string GetAzuriteCommandFilePath()
    {
        var azuriteFileName = "azurite.cmd";
        var azuriteFolderPath = Environment.GetEnvironmentVariable("AzuriteFolderPath");

        return azuriteFolderPath == null
            ? azuriteFileName
            : Path.Combine(azuriteFolderPath, azuriteFileName);
    }

    private static string GetAzuriteArguments(bool useOAuth, bool useSilentMode)
    {
        var arguments = new List<string>();

        if (useOAuth)
        {
            arguments.Add($"--oauth basic --cert {TestCertificateProvider.FilePath} --pwd {TestCertificateProvider.Password}");
        }

        if (useSilentMode)
        {
            arguments.Add("--silent");
        }

        return string.Join(" ", arguments);
    }
}
