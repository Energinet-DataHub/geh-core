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

using System;
using Energinet.DataHub.Core.App.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Core.App.WebApp.Authorization;

public static class AuthorizationExtensions
{
    private const string ScopeClaimType = "scope";

    public static void AddPermissionAuthorization(
        this IServiceCollection services,
        Action<AuthorizationOptions>? configure = default)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in Enum.GetValues<Permission>())
            {
                var policyName = permission.ToString();
                var claimValue = PermissionsAsClaims.Lookup[permission];

                options.AddPolicy(policyName, policyBuilder =>
                {
                    policyBuilder
                        .RequireAuthenticatedUser()
                        .RequireClaim(ScopeClaimType, claimValue);
                });
            }

            configure?.Invoke(options);
        });
    }
}
