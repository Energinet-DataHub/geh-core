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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace ExampleHost.FunctionApp01.Functions;

public class SubsystemAuthenticationFunction
{
    public SubsystemAuthenticationFunction()
    {
    }

    /// <summary>
    /// This method should call "ExampleHost.FunctionApp002.GetAnonymousForSubsystem" without a token.
    /// </summary>
    [Function(nameof(GetAnonymousForSubsystem))]
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
    /// This method should call "ExampleHost.FunctionApp002.GetWithPermissionForSubsystem" with a token.
    /// </summary>
    [Function(nameof(GetWithPermissionForSubsystem))]
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
