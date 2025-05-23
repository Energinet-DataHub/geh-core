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

using System.Security.Claims;

namespace Energinet.DataHub.Core.App.Common.Abstractions.Users;

/// <summary>
/// Creates a subsystem-specific representation of the user through the UserMiddleware.
/// </summary>
public interface IUserProvider<TUser>
    where TUser : class
{
    /// <summary>
    /// Creates a subsystem-specific representation of the user.
    /// </summary>
    /// <param name="userId">The id of the user.</param>
    /// <param name="actorId">The id of the actor.</param>
    /// <param name="multiTenancy">Specifies whether the user has a claim that allows accessing data across market participants.</param>
    /// <param name="claims">The claims present in the token.</param>
    /// <returns>A subsystem-specific representation of the user; or null.</returns>
    Task<TUser?> ProvideUserAsync(
        Guid userId,
        Guid actorId,
        bool multiTenancy,
        IEnumerable<Claim> claims);
}
