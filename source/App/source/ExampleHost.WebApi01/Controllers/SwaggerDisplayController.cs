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

// The "Deprecated = true" has no effect for the swagger UI, since the "TelemetryController" is not marked as deprecated.
[ApiVersion(2.0, Deprecated = true)]
[ApiController]
[Route("webapi01/[controller]")]
public class SwaggerDisplayController : ControllerBase
{
    /// <summary>
    /// The summary is displayed in the swagger UI for the corresponding method. When you have
    /// <Target Name="PrepublishScript" BeforeTargets="PrepareForPublish">
    ///   <ItemGroup>
    ///     <DocFile Include="bin\*\*\*.xml" />
    ///   </ItemGroup>
    ///   <Copy SourceFiles="@(DocFile)" DestinationFolder="$(PublishDir)" SkipUnchangedFiles="false" />
    /// </Target>
    /// in the csproj file.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello from swagger controller in WebApi01");
    }
}
