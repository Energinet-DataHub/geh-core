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

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi01.Controllers;

// If all ApiVersion(X.0)'s has "Deprecated = true" then the swagger UI will have a text stating this.
[ApiVersion(1.0, Deprecated = true)]
[ApiController]
// Every method with the [Obsolete] attribute will have a strikethrough in the swagger UI.
[Obsolete("SwaggerControllerDeprecatedController is deprecated, please use SwaggerControllerController instead.")]
[Route("webapi01/[controller]")]
public class SwaggerDisplayDeprecatedController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello from swagger controller deprecated in WebApi01");
    }
}
