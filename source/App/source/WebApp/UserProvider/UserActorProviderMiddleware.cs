// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
using System;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Middleware.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.App.WebApp.UserProvider;

public class UserActorProviderMiddleware<TUser> : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            var contextAccessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();
            var userActorProvider = context.RequestServices.GetRequiredService<IUserProvider<TUser>>();
            var audience = contextAccessor?.HttpContext?.User.Claims.SingleOrDefault(claim => claim.Type == "aud");
            var user = await userActorProvider.ProvideUserAsync(Guid.Parse(audience!.Value));
            if (user == null)
            {
                HttpContextHelper.SetErrorResponse(context);
                return;
            }

            await next(context).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            HttpContextHelper.SetErrorResponse(context);
        }
    }
}
