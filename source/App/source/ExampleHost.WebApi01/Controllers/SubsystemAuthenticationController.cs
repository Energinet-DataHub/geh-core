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

using ExampleHost.WebApi01.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi01.Controllers;

/// <summary>
/// This class is used for the subsystem-to-subsystem communication scenario (client side)
/// </summary>
/// <remarks>
/// Similar functionality exists for Function App in the 'SubsystemAuthenticationFunction' class
/// located in the 'ExampleHost.FunctionApp01' project.
/// </remarks>
[ApiController]
[Route("webapi01/[controller]")]
public class SubsystemAuthenticationController : ControllerBase
{
    /// <summary>
    /// The http client has been registered and configured to automatically
    /// request a token for the configured scope.
    /// See <see cref="HttpClientExtensions.AddWebApi02HttpClient(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
    /// </summary>
    private readonly HttpClient _webApi02HttpClient;

    public SubsystemAuthenticationController(IHttpClientFactory httpClientFactory)
    {
        _webApi02HttpClient = httpClientFactory.CreateClient(HttpClientNames.WebApi02);
    }

    /// <summary>
    /// This method should call "ExampleHost.WebApi02.GetWithPermissionForSubsystem" with a token
    /// and respond with the same http status code as the endpoint it calls.
    /// </summary>
    [HttpGet("authentication")]
    public async Task<IActionResult> GetWithPermissionForSubsystemAsync()
    {
        var requestIdentification = Guid.NewGuid().ToString();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"authentication/{requestIdentification}");
        using var response = await _webApi02HttpClient.SendAsync(request).ConfigureAwait(false);

        return new StatusCodeResult((int)response.StatusCode);
    }
}
