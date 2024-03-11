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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Tests.Fixtures
{
    public class B2CFixture
    {
        /// <summary>
        /// We are actually using this for integration tests and not system tests,
        /// so we can use any of the allowed environments.
        /// </summary>
        public const string Environment = "u001";

        /// <summary>
        /// We don't require a certain client app, we can use any for which we can
        /// get a valid access token.
        /// </summary>
        public const string SystemOperator = "endk-tso";

        public B2CFixture()
        {
            AuthorizationConfiguration = new B2CAuthorizationConfiguration(
                environment: Environment,
                new List<string> { SystemOperator });

            BackendAppAuthenticationClient = new B2CAppAuthenticationClient(
                AuthorizationConfiguration.TenantId,
                AuthorizationConfiguration.BackendApp,
                AuthorizationConfiguration.ClientApps[SystemOperator]);
        }

        public B2CAuthorizationConfiguration AuthorizationConfiguration { get; }

        public B2CAppAuthenticationClient BackendAppAuthenticationClient { get; }
    }
}
