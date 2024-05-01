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

using System.Net;
using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Core.App.Common.Users;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Energinet.DataHub.Core.App.FunctionApp.Middleware;

/// <summary>
/// If possible, retrieved JWT from header and creates a user, which is then
/// set to the <see cref="UserContext{TUser}"/> and thereby made available
/// for dependency injection in the executing function and other services.
/// </summary>
public class UserMiddleware<TUser> : IFunctionsWorkerMiddleware
    where TUser : class
{
    private const string MultiTenancyClaim = "multitenancy";

    // DO NOT inject scoped services in the middleware constructor.
    // DO use scoped services in middleware by retrieving them from 'FunctionContext.InstanceServices'
    // DO NOT store scoped services in fields or properties of the middleware object. See https://github.com/Azure/azure-functions-dotnet-worker/issues/1327#issuecomment-1434408603
    public UserMiddleware()
    {
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpRequestData = await context.GetHttpRequestDataAsync().ConfigureAwait(false)
            ?? throw new InvalidOperationException("UserMiddleware running without HttpRequestData.");

        var isUserSet = await CanSetUserAsync(context, httpRequestData).ConfigureAwait(false);
        if (isUserSet)
        {
            // Next middleware
            await next(context).ConfigureAwait(false);
        }
        else
        {
            await CreateUnauthorizedResponseAsync(context, httpRequestData).ConfigureAwait(false);
        }
    }

    private static async Task<bool> CanSetUserAsync(FunctionContext context, HttpRequestData httpRequestData)
    {
        var logger = context.GetLogger<UserMiddleware<TUser>>();
        var userProvider = context.InstanceServices.GetRequiredService<IUserProvider<TUser>>();
        var userContext = context.InstanceServices.GetRequiredService<UserContext<TUser>>();

        try
        {
            var token = TryGetTokenFromHeader(httpRequestData);
            if (!string.IsNullOrWhiteSpace(token))
            {
                var tokenHandler = new JsonWebTokenHandler();
                if (tokenHandler.CanReadToken(token))
                {
                    var securityToken = (JsonWebToken)tokenHandler.ReadToken(token);

                    var userId = GetUserId(securityToken.Claims);
                    var actorId = GetActorId(securityToken.Claims);
                    var multiTenancy = GetMultiTenancy(securityToken.Claims);

                    var user = await userProvider
                        .ProvideUserAsync(userId, actorId, multiTenancy, securityToken.Claims)
                        .ConfigureAwait(false);

                    if (user != null)
                    {
                        userContext.SetCurrentUser(user);
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user context from token.");
        }

        return false;
    }

    private static string TryGetTokenFromHeader(HttpRequestData requestData)
    {
        var token = string.Empty;

        if (requestData.Headers.TryGetValues("authorization", out var authorizationHeaders))
        {
            var authorizationHeader = authorizationHeaders.First();
            if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorizationHeader["Bearer ".Length..].Trim();
            }
        }

        return token;
    }

    private static Guid GetUserId(IEnumerable<Claim> claims)
    {
        var userId = claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
        return Guid.Parse(userId);
    }

    private static Guid GetActorId(IEnumerable<Claim> claims)
    {
        var actorId = claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Azp).Value;
        return Guid.Parse(actorId);
    }

    private static bool GetMultiTenancy(IEnumerable<Claim> claims)
    {
        return claims.Any(claim => claim is { Type: MultiTenancyClaim, Value: "true" });
    }

    // Inspired by: https://github.com/Azure/azure-functions-dotnet-worker/blob/main/samples/CustomMiddleware/ExceptionHandlingMiddleware.cs
    private static async Task CreateUnauthorizedResponseAsync(FunctionContext context, HttpRequestData httpRequestData)
    {
        var newHttpResponse = httpRequestData.CreateResponse(HttpStatusCode.Unauthorized);
        // You need to explicitly pass the status code in WriteAsJsonAsync method.
        // https://github.com/Azure/azure-functions-dotnet-worker/issues/776
        await newHttpResponse
            .WriteAsJsonAsync(string.Empty, newHttpResponse.StatusCode)
            .ConfigureAwait(false);

        var invocationResult = context.GetInvocationResult();

        var httpOutputBindingFromMultipleOutputBindings = GetHttpOutputBindingFromMultipleOutputBinding(context);
        if (httpOutputBindingFromMultipleOutputBindings is not null)
        {
            httpOutputBindingFromMultipleOutputBindings.Value = newHttpResponse;
        }
        else
        {
            invocationResult.Value = newHttpResponse;
        }
    }

    // Inspired by: https://github.com/Azure/azure-functions-dotnet-worker/blob/main/samples/CustomMiddleware/ExceptionHandlingMiddleware.cs
    private static OutputBindingData<HttpResponseData>? GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
    {
        // The output binding entry name will be "$return" only when the function return type is HttpResponseData
        var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

        return httpOutputBinding;
    }
}
