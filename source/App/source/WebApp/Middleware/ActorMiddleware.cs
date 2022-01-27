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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.WebApp.Middleware.Helpers;
using Microsoft.AspNetCore.Http;

namespace Energinet.DataHub.Core.App.WebApp.Middleware
{
    public class ActorMiddleware : IMiddleware
    {
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;
        private readonly IActorProvider _actorProvider;
        private readonly IActorContext _actorContext;

        public ActorMiddleware(
            IClaimsPrincipalAccessor claimsPrincipalAccessor,
            IActorProvider actorProvider,
            IActorContext actorContext)
        {
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
            _actorProvider = actorProvider;
            _actorContext = actorContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var claimsPrincipal = _claimsPrincipalAccessor.GetClaimsPrincipal();
            if (claimsPrincipal is null)
            {
                HttpContextHelper.SetErrorResponse(context);
                return;
            }

            var actorIdClaim = GetClaim(claimsPrincipal.Claims, "azp");
            if (!Guid.TryParse(actorIdClaim?.Value, out var actorId))
            {
                HttpContextHelper.SetErrorResponse(context);
                return;
            }

            var actor = await _actorProvider.GetActorAsync(actorId).ConfigureAwait(false);
            _actorContext.CurrentActor = actor;

            await next(context).ConfigureAwait(false);
        }

        private static Claim? GetClaim(IEnumerable<Claim> claims, string claimType)
        {
            return claims.SingleOrDefault(x => x.Type == claimType);
        }
    }
}
