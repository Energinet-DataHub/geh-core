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
using System.Text;
using Energinet.DataHub.Core.TestCommon.Diagnostics;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;

/// <summary>
/// A manager for starting and stopping the Azure Functions host to support integration
/// testing of Azure Functions.
///
/// The host log is written to output using <see cref="ITestDiagnosticsLogger"/>. Read
/// the documentation of that interface to understand where and when the output is available.
/// </summary>
public class FunctionAppHostManager : IDisposable
{
    public FunctionAppHostManager(FunctionAppHostSettings settings, ITestDiagnosticsLogger testLogger)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        TestLogger = testLogger ?? throw new ArgumentNullException(nameof(testLogger));

        HostLogStringBuilder = new StringBuilder();
        HttpClient = CreateHttpClient(Settings.Port);
    }

    public int Port => Settings.Port;

    public HttpClient HttpClient { get; }

    private FunctionAppHostSettings Settings { get; }

    private StringBuilder HostLogStringBuilder { get; }

    private Process? FunctionAppHostProcess { get; set; }

    private ITestDiagnosticsLogger TestLogger { get; }

    public void StartHost()
    {
        if (FunctionAppHostProcess != null)
        {
            throw new InvalidOperationException($"Azure Functions host must be stopped before calling {nameof(StartHost)}.");
        }

        FunctionAppHostProcess = CreateFunctionAppHost(Settings);
        HandleHostStartup();
    }

    public void StopHost()
    {
        if (FunctionAppHostProcess == null)
        {
            return;
        }

        if (!FunctionAppHostProcess.HasExited)
        {
            FunctionAppHostProcess.Kill(entireProcessTree: true);
        }

        FunctionAppHostProcess.Dispose();
        FunctionAppHostProcess = null;
    }

    /// <summary>
    /// Restarts the Function App.
    /// </summary>
    public void RestartHost()
    {
        StopHost();
        StartHost();
    }

    /// <summary>
    /// Only restarts the Function App if merging the existing process environment variables
    /// with incoming environment variables results in any changes.
    /// The merge will add new key/value pairs and update existing keys if their value differs from the incoming.
    /// </summary>
    public void RestartHostIfChanges(IEnumerable<KeyValuePair<string, string>> environmentVariables)
    {
        if (environmentVariables == null)
        {
            throw new ArgumentNullException(nameof(environmentVariables));
        }

        var hasChanges = MergeDictionaries(Settings.ProcessEnvironmentVariables, environmentVariables);
        if (hasChanges)
        {
            StopHost();
            StartHost();
        }
    }

    /// <summary>
    /// Get the collected host process output log.
    /// </summary>
    /// <returns>Snapshot of the collected log.</returns>
    public IReadOnlyList<string> GetHostLogSnapshot()
    {
        string snapshot;
        lock (HostLogStringBuilder)
        {
            snapshot = HostLogStringBuilder.ToString();
        }

        return snapshot
            .Split(Environment.NewLine)
            .ToList();
    }

    /// <summary>
    /// Clear the collected host process output log.
    /// </summary>
    public void ClearHostLog()
    {
        lock (HostLogStringBuilder)
        {
            HostLogStringBuilder.Clear();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        StopHost();
        HttpClient.Dispose();
    }

    private static HttpClient CreateHttpClient(int port)
    {
        return new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{port}", UriKind.Absolute),
        };
    }

    private static Process CreateFunctionAppHost(FunctionAppHostSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DotnetExecutablePath))
        {
            throw new ArgumentException($"{nameof(settings)}.{nameof(settings.DotnetExecutablePath)} == null or only white space.", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.FunctionAppHostPath))
        {
            throw new ArgumentException($"{nameof(settings)}.{nameof(settings.FunctionAppHostPath)} == null or only white space.", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.FunctionApplicationPath))
        {
            throw new ArgumentException($"{nameof(settings)}.{nameof(settings.FunctionApplicationPath)} == null or only white space.", nameof(settings));
        }

        var dotnetExePath = GuardFilePathIsValid(settings.DotnetExecutablePath);
        var functionAppHostPath = GuardFunctionAppHostPathIsValid(settings);
        var functionAppFolder = GuardRelativeFolderPathIsValid(settings.FunctionApplicationPath);

        var process = new Process
        {
            StartInfo =
            {
                FileName = dotnetExePath,
                Arguments = $"\"{functionAppHostPath}\" start -p {settings.Port} --csharp {BuildFunctionsArgument(settings.Functions)}",
                WorkingDirectory = functionAppFolder,
                UseShellExecute = settings.UseShellExecute,
            },
        };

        // StartInfo.UseShellExecute cannot be used in combination with StartInfo.EnvironmentVariables property.
        if (!settings.UseShellExecute)
        {
            // Disable code coverage of the child process.
            // The Code Coverage tool will per default attach itself to the child process.
            // but this is currently not possible, because the called application cannot find
            // Microsoft.VisualStudio.CodeCoverage.Shim assembly.
            // The problem is somewhat described here: https://github.com/Microsoft/vstest/issues/1263
            // The following two lines are necessary but also the settings in .runsettings file in the project root.
            process.StartInfo.EnvironmentVariables["COR_ENABLE_PROFILING"] = "0";
            process.StartInfo.EnvironmentVariables["CORECLR_ENABLE_PROFILING"] = "0";

            foreach (var item in settings.ProcessEnvironmentVariables)
            {
                process.StartInfo.EnvironmentVariables[item.Key] = item.Value;
            }
        }

        // StartInfo.UseShellExecute cannot be used in combination with redirecting output/error.
        if (!settings.UseShellExecute)
        {
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
        }

        return process;
    }

    private static string GuardFunctionAppHostPathIsValid(FunctionAppHostSettings settings)
    {
        var functionAppHostPath = Environment.ExpandEnvironmentVariables(settings.FunctionAppHostPath!);

        return !File.Exists(functionAppHostPath)
            ? throw new FileNotFoundException($"Could not find the Azure Functions Host at '{functionAppHostPath}'. Verify that you have npm and Azure Functions Core Tools installed.", functionAppHostPath)
            : functionAppHostPath;
    }

    private static string GuardFilePathIsValid(string settingName)
    {
        var filePath = Environment.ExpandEnvironmentVariables(settingName);
        return !File.Exists(filePath)
            ? throw new FileNotFoundException("File path does not exist.", filePath)
            : filePath;
    }

    private static string GuardRelativeFolderPathIsValid(string relativeFolderPath)
    {
        var resolvedFolderPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), relativeFolderPath);
        return !Directory.Exists(resolvedFolderPath)
            ? throw new DirectoryNotFoundException($"Relative folder path does not exist. RelativeFolderPath='{relativeFolderPath}'; ResolvedFolderPath='{resolvedFolderPath}'.")
            : resolvedFolderPath;
    }

    private static string BuildFunctionsArgument(string functions)
    {
        return string.IsNullOrWhiteSpace(functions)
            ? string.Empty
            : $"--functions {functions}";
    }

    private bool IsDefaultHostStartedEvent(DataReceivedEventArgs outputEvent)
    {
        return
            outputEvent.Data!.Contains("Host lock lease acquired") // Version >3.0.2996: The functions host does not explicit log a "started" event anymore.
            || outputEvent.Data.Contains("Worker process started and initialized"); // Version >=3.0.3568: When the functions host is ready to serve requests, it will display "Worker process started and initialized".
    }

    private static bool MergeDictionaries(IDictionary<string, string> existing, IEnumerable<KeyValuePair<string, string>> incoming)
    {
        var hasChanges = false;

        foreach (var pair in incoming)
        {
            if (!existing.ContainsKey(pair.Key))
            {
                hasChanges = true;
                existing.Add(pair.Key, pair.Value);
            }
            else if (existing[pair.Key] != pair.Value)
            {
                hasChanges = true;
                existing[pair.Key] = pair.Value;
            }
        }

        return hasChanges;
    }

    private void HandleHostStartup()
    {
        var success = FunctionAppHostProcess!.Start();
        if (!success)
        {
            throw new InvalidOperationException("Could not start Azure Functions host.");
        }

        if (!Settings.UseShellExecute)
        {
            HandleInvisibleHostStartup();
        }
    }

    private void HandleInvisibleHostStartup()
    {
        Func<DataReceivedEventArgs, bool> isStartedEventPredicate = string.IsNullOrWhiteSpace(Settings.HostStartedEvent)
            ? IsDefaultHostStartedEvent
            : IsConfiguredHostStartedEvent;
        var hostStartedListener = new HostStartedOutputListener(FunctionAppHostProcess!, isStartedEventPredicate);

        FunctionAppHostProcess!.OutputDataReceived += OnLogOutputToHostLog;
        FunctionAppHostProcess.OutputDataReceived += OnLogOutputToTestLogger;

        FunctionAppHostProcess.BeginOutputReadLine();
        var timeout = TimeSpan.FromSeconds(Settings.MaxWaitSeconds);
        var isHostStarted = hostStartedListener.WaitForStarted(timeout);

        // If Host process fails on load
        if (FunctionAppHostProcess.HasExited)
        {
            var error = FunctionAppHostProcess.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"Azure Functions host failed with: '{error}'. Try looking into the host output written at startup. For more on how to get the output, read the documentation on the interface ITestDiagnosticsLogger.");
            }
        }

        // If not Host is started within timeout
        if (!isHostStarted)
        {
            throw new InvalidOperationException($"Could not start Azure Functions host within timeout '{timeout}'. Try looking into the host output written at startup. For more on how to get the output, read the documentation on the interface ITestDiagnosticsLogger.");
        }

        TestLogger.WriteLine($"Started host with process id '{FunctionAppHostProcess.Id}'");
    }

    private bool IsConfiguredHostStartedEvent(DataReceivedEventArgs outputEvent)
    {
        return outputEvent.Data!.Contains(Settings.HostStartedEvent);
    }

    private void OnLogOutputToHostLog(object sender, DataReceivedEventArgs outputEvent)
    {
        if (outputEvent.Data != null)
        {
            lock (HostLogStringBuilder)
            {
                HostLogStringBuilder.AppendLine(outputEvent.Data);
            }
        }
    }

    private void OnLogOutputToTestLogger(object sender, DataReceivedEventArgs outputEvent)
    {
        if (outputEvent.Data != null)
        {
            TestLogger.WriteLine(outputEvent.Data);
        }
    }
}
