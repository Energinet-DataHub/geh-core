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

using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using ExampleHost.FunctionApp01.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace ExampleHost.FunctionApp01.Functions;

/// <summary>
/// Similar functionality exists for Web App in the 'AuthenticationController' class
/// located in the 'ExampleHost.WebApi03' project.
/// </summary>
public class AuthenticationFunction
{
    private readonly IUserContext<ExampleSubsystemUser> _userContext;

    public AuthenticationFunction(IUserContext<ExampleSubsystemUser> userContext)
    {
        _userContext = userContext;
    }

    /// <summary>
    /// 1: This method should not require any 'Bearer' token in the 'Authorization' header.
    ///   It should allow anonymous access and always return the given Guid, for tests to verify.
    /// </summary>
    [Function(nameof(GetAnonymous))]
    [AllowAnonymous]
    public IActionResult GetAnonymous(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authentication/anon/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }

    /// <summary>
    /// 1: This method should be called with a 'Bearer' token in the 'Authorization' header.
    ///   The token must be a nested token (containing both external and internal token).
    /// 2: The DarkLoop Authorization extension in combination with ASP.NET Core authentication classes
    ///   should retrieve the token and validate it.
    /// 3: If successfull the given Guid is returned, for tests to verify.
    /// </summary>
    [Function(nameof(GetWithPermission))]
    [Authorize]
    public IActionResult GetWithPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authentication/auth/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }

    /// <summary>
    /// 1: This method should be called with a 'Bearer' token in the 'Authorization' header.
    ///   The token must be a nested token (containing both external and internal token).
    /// 2: The "UserMiddleware" should retrieve the user information from this token, and
    ///   assign it to the "UserContext" (a scoped service).
    /// 3: The "IUserContext" (a scoped service) can then be injected to the function class
    ///   and give access to the stored user information.
    /// 4: If successfull the UserId is retrieved and returned, for tests to verify.
    ///   If the user is not available an Empty guid is returned.
    /// </summary>
    [Function(nameof(GetUserWithPermission))]
    [Authorize]
    public IActionResult GetUserWithPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authentication/user")]
        HttpRequest httpRequest,
        FunctionContext context)
    {
        try
        {
            return new OkObjectResult(_userContext.CurrentUser.UserId.ToString());
        }
        catch
        {
            return new OkObjectResult(Guid.Empty.ToString());
        }
    }
}
