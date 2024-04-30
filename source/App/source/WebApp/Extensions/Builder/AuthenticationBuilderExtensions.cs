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

using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Energinet.DataHub.Core.App.WebApp.Extensions.Builder;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/>
/// that allow adding authentication related middleware to an ASP.NET Core app.
/// </summary>
public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Register middleware necessary for enabling user authentication in an ASP.NET Core app.
    /// </summary>
    public static IApplicationBuilder UseUserMiddlewareForWebApp<TUser>(this IApplicationBuilder app)
        where TUser : class
    {
        app.UseMiddleware<UserMiddleware<TUser>>();

        return app;
    }
}
