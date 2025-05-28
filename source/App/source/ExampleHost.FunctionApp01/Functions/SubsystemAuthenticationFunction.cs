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

using ExampleHost.FunctionApp01.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace ExampleHost.FunctionApp01.Functions;

/// <summary>
/// This class is used for the subsystem-to-subsystem communication scenario (client side)
/// </summary>
/// <remarks>
/// Similar functionality exists for Web App in the 'SubsystemAuthenticationController' class
/// located in the 'ExampleHost.WebApi01' project.
/// </remarks>
public class SubsystemAuthenticationFunction
{
    /// <summary>
    /// The http client has been registered and configured to automatically
    /// request a token for the configured scope.
    /// See <see cref="HttpClientExtensions.AddApp02HttpClient(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
    /// </summary>
    private readonly HttpClient _app02ApiHttpClient;

    public SubsystemAuthenticationFunction(IHttpClientFactory httpClientFactory)
    {
        _app02ApiHttpClient = httpClientFactory.CreateClient(HttpClientNames.App02Api);
    }

    /// <summary>
    /// This method should call "ExampleHost.FunctionApp02.GetWithPermissionForSubsystem" with a token
    /// and respond with the same http status code as the endpoint it calls.
    /// </summary>
    [Function(nameof(GetWithPermissionForSubsystemAsync))]
    public async Task<IActionResult> GetWithPermissionForSubsystemAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "subsystemauthentication/authentication")]
        HttpRequest httpRequest)
    {
        var requestIdentification = Guid.NewGuid().ToString();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/subsystemauthentication/authentication/{requestIdentification}");
        using var response = await _app02ApiHttpClient.SendAsync(request).ConfigureAwait(false);

        return new StatusCodeResult((int)response.StatusCode);
    }
}
