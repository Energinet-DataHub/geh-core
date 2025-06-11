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

using System.Net.Http.Headers;

namespace Energinet.DataHub.Core.App.Common.Identity;

public interface IAuthorizationHeaderProvider
{
    /// <summary>
    /// Create an authorization header to be used when calling another subsystem
    /// in subsystem-to-subsystem communication.
    /// </summary>
    /// <param name="scope">
    /// The scope requested to access a protected API.
    /// The scope should be of the form "{ResourceIdUri/.default}" for instance
    /// <c>https://management.azure.net/.default</c>.
    /// You can request a token for only one resource at a time; use
    /// several calls to get tokens for other resources.
    /// </param>
    /// <param name="cancellationToken">Token for cancelling operation.</param>
    Task<AuthenticationHeaderValue> CreateAuthorizationHeaderAsync(string scope, CancellationToken cancellationToken);
}
