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
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights.WindowsServer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ExampleHost.FunctionApp01.Functions;

public class RestApiExampleFunction
{
    private readonly ILogger _logger;
    private readonly ServiceBusSender _serviceBusSender;

    public RestApiExampleFunction(ILogger<RestApiExampleFunction> logger, ServiceBusSender serviceBusSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceBusSender = serviceBusSender ?? throw new ArgumentNullException(nameof(serviceBusSender));
    }

    [Function(nameof(TelemetryAsync))]
    public async Task<HttpResponseData> TelemetryAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "v1/telemetry")]
        HttpRequestData httpRequest)
    {
        _logger.LogInformation($"ExampleHost {nameof(TelemetryAsync)}: We should be able to find this log message by following the trace of the request.");

        await SendServiceBusMessageAsync(nameof(TelemetryAsync)).ConfigureAwait(false);

        return CreateResponse(httpRequest);
    }

    /// <summary>
    /// Send a Service Bus message for another host to receive.
    /// In Application Insights we should be able to trace the request end-to-end
    /// and see it reach the current Host as well as the Host containing the
    /// Service Bus trigger.
    /// </summary>
    private Task SendServiceBusMessageAsync(string messageContent)
    {
        return _serviceBusSender.SendMessageAsync(new ServiceBusMessage(messageContent));
    }

    private static HttpResponseData CreateResponse(HttpRequestData httpRequest)
    {
        return httpRequest.CreateResponse(HttpStatusCode.Accepted);
    }
}
