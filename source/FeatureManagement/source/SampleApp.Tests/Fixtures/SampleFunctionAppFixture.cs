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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FeatureManagement.SampleApp.Common;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Energinet.DataHub.Core.FeatureManagement.SampleApp.Tests.Fixtures
{
    public class SampleFunctionAppFixture : FunctionAppFixture
    {
        public SampleFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
        }

        /// <summary>
        /// The setting name of the <see cref="FeatureFlags.Names.UseGuidMessage"/> feature flag.
        /// </summary>
        public string UseGuidMessageSettingName => $"{FeatureFlags.ConfigurationPrefix}{FeatureFlags.Names.UseGuidMessage}";

        /// <summary>
        /// The setting name of the CreateMessage (function) disabled flag.
        /// </summary>
        public string CreateMessageDisabledSettingName => "AzureWebJobs.CreateMessage.Disabled";

        private AzuriteManager AzuriteManager { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
            {
                return;
            }

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\SampleApp\\bin\\{buildConfiguration}\\net8.0";

            hostSettings.ProcessEnvironmentVariables.Add(EnvironmentSettingNames.AzureWebJobsStorage, "UseDevelopmentStorage=true");
            hostSettings.ProcessEnvironmentVariables.Add(EnvironmentSettingNames.FunctionWorkerRuntime, "dotnet-isolated");

            // The default endpoint for Azure Functions when using the HTTP trigger
            hostSettings.Port = 7071;

            // => Feature flags
            hostSettings.ProcessEnvironmentVariables.Add(UseGuidMessageSettingName, "false");

            // => Disabled flags
            hostSettings.ProcessEnvironmentVariables.Add(CreateMessageDisabledSettingName, "false");
        }

        /// <inheritdoc/>
        protected override Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override Task OnDisposeFunctionAppDependenciesAsync()
        {
            AzuriteManager.Dispose();

            return Task.CompletedTask;
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
