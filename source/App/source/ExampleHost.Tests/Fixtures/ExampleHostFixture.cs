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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using ExampleHost.FunctionApp01.Common;
using Xunit;
using Xunit.Abstractions;

namespace ExampleHost.Tests.Fixtures
{
    /// <summary>
    /// Support testing flows between multiple Function App hosts.
    /// </summary>
    public class ExampleHostFixture : IAsyncLifetime
    {
        public ExampleHostFixture()
        {
            TestLogger = new TestDiagnosticsLogger();

            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();

            HostConfigurationBuilder = new FunctionAppHostConfigurationBuilder();
        }

        public ITestDiagnosticsLogger TestLogger { get; }

        [NotNull]
        public FunctionAppHostManager? App01HostManager { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private FunctionAppHostConfigurationBuilder HostConfigurationBuilder { get; }

        public Task InitializeAsync()
        {
            // => Storage emulator
            AzuriteManager.StartAzurite();

            // => Prepare host settings
            var localSettingsSnapshot = HostConfigurationBuilder.BuildLocalSettingsConfiguration();

            var app01HostSettings = HostConfigurationBuilder.CreateFunctionAppHostSettings();

            var buildConfiguration = GetBuildConfiguration();
            var port = 8000;
            app01HostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\ExampleHost.FunctionApp01\\bin\\{buildConfiguration}\\net6.0";
            app01HostSettings.Functions = "CreatePetAsync";
            app01HostSettings.Port = ++port;

            app01HostSettings.ProcessEnvironmentVariables.Add(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
            app01HostSettings.ProcessEnvironmentVariables.Add(EnvironmentSettingNames.AppInsightsInstrumentationKey, IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);

            App01HostManager = new FunctionAppHostManager(app01HostSettings, TestLogger);
            StartHost(App01HostManager);

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            App01HostManager.Dispose();

            AzuriteManager.Dispose();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Use this method to attach <paramref name="testOutputHelper"/> to the host logging pipeline.
        /// While attached, any entries written to host log pipeline will also be logged to xUnit test output.
        /// It is important that it is only attached while a test i active. Hence, it should be attached in
        /// the test class constructor; and detached in the test class Dispose method (using 'null').
        /// </summary>
        /// <param name="testOutputHelper">If a xUnit test is active, this should be the instance of xUnit's <see cref="ITestOutputHelper"/>; otherwise it should be 'null'.</param>
        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            TestLogger.TestOutputHelper = testOutputHelper;
        }

        private static void StartHost(FunctionAppHostManager hostManager)
        {
            IEnumerable<string> hostStartupLog;

            try
            {
                hostManager.StartHost();
            }
            catch (Exception)
            {
                // Function App Host failed during startup.
                // Exception has already been logged by host manager.
                hostStartupLog = hostManager.GetHostLogSnapshot();

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                // Rethrow
                throw;
            }

            // Function App Host started.
            hostStartupLog = hostManager.GetHostLogSnapshot();
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
