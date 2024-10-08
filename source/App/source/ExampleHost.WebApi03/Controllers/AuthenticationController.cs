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
using ExampleHost.WebApi03.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi03.Controllers;

/// <summary>
/// Similar functionality exists for Function App in the 'AuthenticationFunction' class
/// located in the 'ExampleHost.FunctionApp01' project.
/// </summary>
[ApiController]
[Route("webapi03/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserContext<ExampleSubsystemUser> _userContext;

    public AuthenticationController(IUserContext<ExampleSubsystemUser> userContext)
    {
        _userContext = userContext;
    }

    /// <summary>
    /// 1: This method should not require any 'Bearer' token in the 'Authorization' header.
    ///   It should allow anonymous access and always return the given Guid, for tests to verify.
    /// </summary>
    [HttpGet("anon/{identification}")]
    [AllowAnonymous]
    public string Get(string identification)
    {
        return identification;
    }

    /// <summary>
    /// 1: This method should be called with a 'Bearer' token in the 'Authorization' header.
    ///   The token must be a nested token (containing both external and internal token).
    /// 2: ASP.NET Core authentication classes should retrieve the token and validate it.
    /// 3: If successfull the given Guid is returned, for tests to verify.
    /// </summary>
    [HttpGet("auth/{identification}")]
    [Authorize]
    public string GetWithPermission(string identification)
    {
        return identification;
    }

    /// <summary>
    /// 1: This method should be called with a 'Bearer' token in the 'Authorization' header.
    ///   The token must be a nested token (containing both external and internal token).
    /// 2: The "UserMiddleware" should retrieve the user information from this token, and
    ///   assign it to the "UserContext" (a scoped service).
    /// 3: The "IUserContext" (a scoped service) can then be injected to the controller class
    ///   and give access to the stored user information.
    /// 4: If successfull the UserId is retrieved and returned, for tests to verify.
    ///   If the user is not available an Empty guid is returned.
    /// </summary>
    [HttpGet("user")]
    [Authorize]
    public string GetUserWithPermission()
    {
        return _userContext.CurrentUser.UserId.ToString();
    }
}
