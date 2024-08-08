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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace ExampleHost.FunctionApp01.Functions;

/// <summary>
/// Similar functionality exists for Web App in the 'AuthorizationController' class
/// located in the 'ExampleHost.WebApi03' project.
/// </summary>
public class AuthorizationFunction
{
    [Function(nameof(GetOrganizationReadPermission))]
    [Authorize(Roles = "organizations:view")]
    public IActionResult GetOrganizationReadPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authorization/org/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }

    /// <summary>
    /// Require user to be in one of the roles (Or)
    /// </summary>
    [Function(nameof(GetOrganizationOrGridAreasPermission))]
    [Authorize(Roles = "organizations:view, grid-areas:manage")]
    public IActionResult GetOrganizationOrGridAreasPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authorization/org_or_grid/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }

    /// <summary>
    /// Require user to be in both roles (And)
    /// </summary>
    [Function(nameof(GetOrganizationAndGridAreasPermission))]
    [Authorize(Roles = "organizations:view")]
    [Authorize(Roles = "grid-areas:manage")]
    public IActionResult GetOrganizationAndGridAreasPermission(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "authorization/org_and_grid/{identification:guid}")]
        HttpRequest httpRequest,
        Guid identification)
    {
        return new OkObjectResult(identification.ToString());
    }
}
