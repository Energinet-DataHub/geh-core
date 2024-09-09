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

using System.Text;
using ExampleHost.WebApi01.Common;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi01.Controllers;

[ApiController]
[Route("webapi01/[controller]")]
public class SwaggerHandleEnumController : ControllerBase
{
    private readonly ILogger<TelemetryController> _logger;

    public SwaggerHandleEnumController(ILogger<TelemetryController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public Task<EnumTest> GetTestEnum()
    {
        return Task.FromResult(EnumTest.First);
    }
}
