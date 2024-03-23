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

using ExampleHost.WebApi01.Common;
using Microsoft.AspNetCore.Mvc;

namespace ExampleHost.WebApi01.Controllers;

[ApiController]
[Route("webapi01/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ILogger<TelemetryController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public TelemetryController(ILogger<TelemetryController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    [HttpGet("{identification}")]
    public async Task<string> GetAsync(string identification)
    {
        var traceparent = HttpContext.Request.Headers["traceparent"].ToString();
        _logger.LogInformation($"ExampleHost WebApi01 {identification} Information: We should be able to find this log message by following the trace of the request '{traceparent}'.");
        _logger.LogWarning($"ExampleHost WebApi01 {identification} Warning: We should be able to find this log message by following the trace of the request '{traceparent}'.");

        var httpClient = _httpClientFactory.CreateClient(HttpClientNames.WebApi02);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi02/telemetry/{identification}");
        using var response = await httpClient.SendAsync(request).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}
