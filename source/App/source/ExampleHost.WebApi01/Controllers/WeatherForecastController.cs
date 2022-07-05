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

namespace ExampleHost.WebApi01.Controllers
{
    [ApiController]
    [Route("webapi01/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        [HttpGet("{identification}")]
        public async Task<string> GetAsync(string identification)
        {
            _logger.LogInformation($"ExampleHost WebApi01 {identification}: We should be able to find this log message by following the trace of the request.");
            _logger.LogWarning($"ExampleHost WebApi01 {identification}: We should be able to find this log message by following the trace of the request.");

            using var httpClient = _httpClientFactory.CreateClient(HttpClientNames.WebApi02);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"webapi02/weatherforecast/{identification}");
            var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
