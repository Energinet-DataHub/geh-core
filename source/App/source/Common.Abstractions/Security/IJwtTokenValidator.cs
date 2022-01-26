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
using System.Threading.Tasks;

namespace Energinet.DataHub.Core.App.Common.Abstractions.Security
{
    /// <summary>
    /// JWT token validator
    /// </summary>
    public interface IJwtTokenValidator
    {
        /// <summary>
        /// Perform validation of a JWT token
        /// </summary>
        /// <param name="token"></param>
        /// <returns>
        /// <para>'false' if the token is null, empty, malformed or could not be validated.</para>
        /// <para>'true' if the token is valid followed by an instance of ClaimsPrincipal.</para>
        /// </returns>
        Task<(bool IsValid, ClaimsPrincipal? ClaimsPrincipal)> ValidateTokenAsync(string? token);
    }
}
