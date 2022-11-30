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
using Energinet.DataHub.Core.App.Common.Abstractions.Users;

namespace ExampleHost.WebApi04.Security;

public sealed class ExampleDomainUserProvider : IUserProvider<ExampleDomainUser>
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ExampleDomainUserProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public Task<ExampleDomainUser?> ProvideUserAsync(
        Guid userId,
        Guid actorId,
        bool isFas,
        IEnumerable<Claim> claims)
    {
        return _contextAccessor!.HttpContext!.Request.Headers.ContainsKey("DenyUser")
            ? Task.FromResult<ExampleDomainUser?>(null)
            : Task.FromResult<ExampleDomainUser?>(new ExampleDomainUser(userId, actorId));
    }
}
