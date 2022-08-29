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

namespace Energinet.DataHub.Core.App.Common.Tests.Fixtures
{
    /// <summary>
    /// Name and credential settings for a B2C client app to be used for testing.
    /// </summary>
    /// <param name="Name">Name of the client app.</param>
    /// <param name="CredentialSettings">Credential settings necessary for acquiring an access token for the client app.</param>
    public record B2CClientAppSettings(string Name, B2CClientAppCredentialsSettings CredentialSettings);
}