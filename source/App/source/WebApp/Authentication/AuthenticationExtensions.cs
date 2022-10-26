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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Energinet.DataHub.Core.App.WebApp.Authentication;

public static class AuthenticationExtensions
{
    public static void AddJwtBearerAuthentication(
        this IServiceCollection services,
        string metadataAddress,
        Func<IEnumerable<string>, bool> audienceValidator)
    {
        ArgumentNullException.ThrowIfNull(metadataAddress);
        ArgumentNullException.ThrowIfNull(audienceValidator);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever());

                var tokenParams = options.TokenValidationParameters;
                tokenParams.ValidateAudience = true;
                tokenParams.ValidateIssuer = true;
                tokenParams.ValidateIssuerSigningKey = true;
                tokenParams.ValidateLifetime = true;
                tokenParams.RequireSignedTokens = true;
                tokenParams.ClockSkew = TimeSpan.Zero;
                tokenParams.AudienceValidator = (audiences, _, _) =>
                {
                    var audiencesList = audiences.ToList();
                    return audiencesList.Any() && audienceValidator(audiencesList);
                };
            });
    }
}
