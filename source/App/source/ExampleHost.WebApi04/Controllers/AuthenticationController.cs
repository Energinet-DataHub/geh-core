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

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi04.Controllers;

[ApiController]
[Route("webapi04/[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpGet("anon/{identification}")]
    public string Get(string identification)
    {
        return identification;
    }

    [HttpGet("auth/{identification}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public string GetWithPermission(string identification)
    {
        return identification;
    }

    [HttpGet("user")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public string GetUserWithPermission()
    {
        return User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
    }
}
