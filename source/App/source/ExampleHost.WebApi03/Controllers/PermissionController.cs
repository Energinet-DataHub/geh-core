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
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi03.Controllers;

[ApiController]
[Route("webapi03/[controller]")]
public class PermissionController : ControllerBase
{
    [HttpGet("anon/{identification}")]
    [AllowAnonymous]
    public string Get(string identification)
    {
        return identification;
    }

    [HttpGet("org/{identification}")]
    [Authorize(Roles = "organizations:view")]
    public string GetOrganizationReadPermission(string identification)
    {
        return identification;
    }

    [HttpGet("grid/{identification}")]
    [Authorize(Roles = "grid-areas:manage")]
    public string GetGridAreaPermission(string identification)
    {
        return identification;
    }

    [HttpGet("org_or_grid/{identification}")]
    [Authorize(Roles = "organizations:view, grid-areas:manage")]
    public string GetOrganizationOrGridAreasPermission(string identification)
    {
        return identification;
    }

    [HttpGet("org_and_grid/{identification}")]
    [Authorize(Roles = "organizations:view")]
    [Authorize(Roles = "grid-areas:manage")]
    public string GetOrganizationAndGridAreasPermission(string identification)
    {
        return identification;
    }
}
