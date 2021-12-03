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

using System.Collections.Generic;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost
{
    /// <summary>
    /// Settings used by <see cref="FunctionAppHostManager"/> when starting the
    /// Azure Functions host to support integration testing of Azure Functions.
    /// </summary>
    public class FunctionAppHostSettings
    {
        /// <summary>
        /// The full path to the .NET Core exe (dotnet.exe) file.
        /// </summary>
        public string? DotnetExecutablePath { get; set; }

        /// <summary>
        /// The full path to the Azure Functions Host (func.dll) file.
        /// </summary>
        public string? FunctionAppHostPath { get; set; }

        /// <summary>
        /// Relative path from the working folder of the test project to the Function App under test.
        ///
        /// IMPORTANT:
        /// You will need to update this according to the build configuration (Debug/Release)
        /// and .NET Core version.
        /// </summary>
        public string? FunctionApplicationPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the operating system shell to start the process.
        ///
        /// If "true" the shell will be visible while the host is running, making it possible to see the
        /// functions output (logs) in the console for e.g. debugging issues.
        /// </summary>
        public bool UseShellExecute { get; set; }
            = false;

        /// <summary>
        /// Port on which the host will listen.
        /// </summary>
        public int Port { get; set; }
            = 7001;

        /// <summary>
        /// After starting the host this is the max. time we wait before we expect the
        /// host to be ready to serve requests.
        /// When the host is ready we allow the tests to continue and let the clients call it.
        /// If the host is not ready within expected time, an exception is thrown.
        /// </summary>
        public int MaxWaitSeconds { get; set; }
            = 120;

        /// <summary>
        /// A space separated list of functions to load. If empty all functions will be loaded.
        /// </summary>
        public string Functions { get; set; }
            = string.Empty;

        /// <summary>
        /// Only support if <see cref="UseShellExecute"/> is "false".
        /// A dictionary of environment variable that will we set for the Function App host process.
        /// </summary>
        public IDictionary<string, string> ProcessEnvironmentVariables { get; set; }
            = new Dictionary<string, string>();

        /// <summary>
        /// The log message that indicates the Azure Functions host or worker is started and ready to server.
        /// During startup the host manager blocks until this message is logged, or a timeout occurs.
        /// Normally the default values implemented in code is sufficient; but sometimes it is necessary
        /// to have full control of which message to expect.
        /// If empty the current default values implemented in code will be used.
        /// </summary>
        public string HostStartedEvent { get; set; }
            = string.Empty;
    }
}
