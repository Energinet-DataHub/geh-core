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

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C
{
    /// <summary>
    /// Id and scope for a B2C application to which we want access by using an access token.
    /// </summary>
    public record B2CAppSettings
    {
        public B2CAppSettings(string appId)
        {
            AppId = appId;
            AppScope = new[] { $"{AppId}/.default" };
        }

        /// <summary>
        /// Id of the application.
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// The scope for which we must request an access token, to be authorized by the API Management.
        /// </summary>
        public IEnumerable<string> AppScope { get; }
    }
}
