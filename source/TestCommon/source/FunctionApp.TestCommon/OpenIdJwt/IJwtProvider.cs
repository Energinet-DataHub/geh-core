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

using System.Security.Claims;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;

namespace Energinet.DataHub.Core.FunctionApp.TestCommon.OpenIdJwt;

/// <summary>
/// Provides methods for creating DH3 internal tokens and fake tokens, that can be used for
/// testing endpoints with authorization and authentication.
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// Creates an internal token valid for DataHub3 applications, containing the following claims:
    /// - "token" claim which is an external token retrieved from Microsoft Entra (configured in the given <see cref="AzureB2CSettings"/>)
    /// - "sub" claim specified in the <paramref name="userId"/> parameter
    /// - "azp" claim specified in the <paramref name="actorId"/> parameter
    /// - "role" claims for each role specified in the <paramref name="roles"/> parameter
    /// - Any extra claims specified in the <paramref name="extraClaims"/> parameter
    /// </summary>
    /// <param name="userId">The user id value written to the 'sub' claim in the internal token.</param>
    /// <param name="actorId">The actor id value written to the 'azp' claim in the internal token.</param>
    /// <param name="roles">Optional roles to add as "role" claims in the internal token. When running in Azure this could be something like "calculations:manage".</param>
    /// <param name="extraClaims">Optional extra claims to add to the internal token.</param>
    /// <returns>The internal token which wraps the provided external token.</returns>
    Task<string> CreateInternalTokenAsync(
        string userId, // TODO: Is it possible to override these, or are they bound to the external token?
        string actorId, // TODO: Is it possible to override these, or are they bound to the external token?
        string[]? roles = null,
        Claim[]? extraClaims = null);

    /// <summary>
    /// Create a fake token which cannot be verified by DH3 applications. This can be used for testing
    /// that a client cannot authorize using an incorrect token.
    /// </summary>
    /// <returns>The token is returned in string format, without the "bearer" prefix</returns>
    string CreateFakeToken();
}
