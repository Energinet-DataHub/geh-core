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

/// <summary>
/// This class is used for the subsystem-to-subsystem communication scenario (client side)
/// </summary>
public class SubsystemAuthenticationFunction
{
    public SubsystemAuthenticationFunction()
    {
    }

    /// <summary>
    /// This method should call "ExampleHost.FunctionApp002.GetAnonymousForSubsystem" without a token
    /// and respond with the same http status code as the endpoint it calls.
    /// </summary>
    [Function(nameof(GetAnonymousForSubsystem))]
    public IActionResult GetAnonymousForSubsystem(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "subsystemauthentication/anonymous")]
        HttpRequest httpRequest)
    {
        // TODO: Inject client or service by which we can make call to App002.
        // TODO: Wait for response and respond to the test with the same http status code.
        return new OkResult();
    }

    /// <summary>
    /// This method should call "ExampleHost.FunctionApp002.GetWithPermissionForSubsystem" with a token
    /// and respond with the same http status code as the endpoint it calls.
    /// </summary>
    [Function(nameof(GetWithPermissionForSubsystem))]
    public IActionResult GetWithPermissionForSubsystem(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "subsystemauthentication/authentication/{requestToken:bool}")]
        HttpRequest httpRequest,
        bool requestToken)
    {
        // TODO: Inject client or service by which we can make call to App002.
        // TODO: Add a token to the request, based on the given parameter.
        // TODO: Wait for response and respond to the test with the same http status code.
        return new OkResult();
    }
}
