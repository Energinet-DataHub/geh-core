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

using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using HealthChecks.UI.Core;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;

public class HealthCheckEndpointHandler : IHealthCheckEndpointHandler
{
    public HealthCheckEndpointHandler(HealthCheckService healthCheckService)
    {
        HealthCheckService = healthCheckService;
        JsonOptions = CreateJsonOptions();
    }

    private HealthCheckService HealthCheckService { get; }

    private JsonSerializerOptions JsonOptions { get; }

    public async Task<HttpResponseData> HandleAsync(HttpRequestData httpRequest, string endpoint)
    {
        var predicate = DeterminePredicateFromEndpoint(endpoint);
        if (predicate == null)
        {
            return httpRequest.CreateResponse(HttpStatusCode.NotFound);
        }
        else
        {
            var result = await HealthCheckService.CheckHealthAsync(predicate).ConfigureAwait(false);

            var httpResponse = httpRequest.CreateResponse();
            httpResponse.StatusCode = result.Status == HealthStatus.Healthy
                ? HttpStatusCode.OK
                : HttpStatusCode.ServiceUnavailable;

            await WriteUICompatibleResponseAsync(httpResponse, result).ConfigureAwait(false);

            return httpResponse;
        }
    }

    private static Func<HealthCheckRegistration, bool>? DeterminePredicateFromEndpoint(string endpoint)
    {
        Func<HealthCheckRegistration, bool>? predicate = null;

        if (string.Compare(endpoint, "live", ignoreCase: true) == 0)
        {
            predicate = r => r.Name.Contains(HealthChecksConstants.LiveHealthCheckName);
        }

        if (string.Compare(endpoint, "ready", ignoreCase: true) == 0)
        {
            predicate = r => !r.Name.Contains(HealthChecksConstants.LiveHealthCheckName);
        }

        return predicate;
    }

    /// <summary>
    /// Write response compatible with the Health Checks UI.
    /// </summary>
    private async Task WriteUICompatibleResponseAsync(HttpResponseData httpResponse, HealthReport report)
    {
        httpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");

#pragma warning disable SA1305 // Field names should not use Hungarian notation
        var uiReport = UIHealthReport.CreateFrom(report);
#pragma warning restore SA1305 // Field names should not use Hungarian notation

        await JsonSerializer.SerializeAsync(httpResponse.Body, uiReport, JsonOptions).ConfigureAwait(false);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        options.Converters.Add(new JsonStringEnumConverter());

        // for compatibility with older UI versions ( <3.0 ) we arrange
        // timespan serialization as s
        options.Converters.Add(new TimeSpanConverter());

        return options;
    }

    private class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
