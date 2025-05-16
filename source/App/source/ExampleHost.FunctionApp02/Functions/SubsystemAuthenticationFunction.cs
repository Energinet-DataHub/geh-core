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

using Energinet.DataHub.Core.App.Common.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace ExampleHost.FunctionApp02.Functions;

/// <summary>
/// This class is used for the subsystem-to-subsystem communication scenario (server side)
/// </summary>
public class SubsystemAuthenticationFunction
{
    /// <summary>
    /// 1: This method should not require any 'Bearer' token in the 'Authorization' header.
    ///   It should allow anonymous access and always return the given Guid, for tests to verify.
    /// </summary>
    [Function(nameof(GetAnonymousForSubsystem))]
    [AllowAnonymous]
    public IActionResult GetAnonymousForSubsystem(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "subsystemauthentication/anonymous/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }

    /// <summary>
    /// 1: This method should be called with a 'Bearer' token in the 'Authorization' header.
    ///   The token must be a standard JWT with the expected "scope" as configured by <see cref="SubsystemAuthenticationOptions"/>.
    /// 2: The DarkLoop Authorization extension in combination with ASP.NET Core authentication classes
    ///   should retrieve the token and validate it.
    /// 3: If successfull the given Guid is returned, for tests to verify.
    /// </summary>
    [Function(nameof(GetWithPermissionForSubsystem))]
    [Authorize]
    public IActionResult GetWithPermissionForSubsystem(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "subsystemauthentication/authentication/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }
}
