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

using Azure.Core;
using Azure.Identity;

namespace Energinet.DataHub.Core.Databricks.Jobs.Http;

/// <inheritdoc cref="ITokenProvider"/>
public class ServicePrincipalTokenProvider : ITokenProvider
{
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var credential = new ManagedIdentityCredential();

        // The scope is a fixed value for Databricks in Azure
        var tokenRequestContext = new TokenRequestContext(["2ff814a6-3304-4ab8-85cb-cd0e6f879c1d/.default"]);
        var token = await credential.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);

        return token.Token;
    }
}
