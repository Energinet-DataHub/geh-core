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

using System.IdentityModel.Tokens.Jwt;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SimpleInjector;

namespace Energinet.DataHub.Core.App.WebApp.SimpleInjector
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// <param name="container">Simple Injector Container</param>
        /// <param name="metadataAddress">OpenID Configuration URL used for acquiring metadata</param>
        /// <param name="audience">Audience used for validation of JWT token</param>
        public static void AddJwtTokenSecurity(this Container container, string metadataAddress, string audience)
        {
            container.Register<ISecurityTokenValidator, JwtSecurityTokenHandler>(Lifestyle.Singleton);
            container.Register<IConfigurationManager<OpenIdConnectConfiguration>>(
                () => new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever()),
                Lifestyle.Singleton);

            container.Register<IJwtTokenValidator>(
                () => new JwtTokenValidator(
                    container.GetRequiredService<ILogger<JwtTokenValidator>>(),
                    container.GetRequiredService<ISecurityTokenValidator>(),
                    container.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                    audience),
                Lifestyle.Scoped);

            container.Register<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>(Lifestyle.Scoped);
            container.Register<ClaimsPrincipalContext>(Lifestyle.Scoped);

            container.Register<JwtTokenMiddleware>(Lifestyle.Scoped);
        }

        public static void AddUserAuthentication<TUser, TUserProvider>(this Container container)
            where TUser : class
            where TUserProvider : class, IUserProvider<TUser>
        {
            container.Register<UserContext<TUser>>(Lifestyle.Scoped);
            container.Register<IUserContext<TUser>>(container.GetRequiredService<UserContext<TUser>>, Lifestyle.Scoped);
            container.Register<IUserProvider<TUser>, TUserProvider>(Lifestyle.Scoped);
            container.Register<UserMiddleware<TUser>>(Lifestyle.Scoped);
        }
    }
}
