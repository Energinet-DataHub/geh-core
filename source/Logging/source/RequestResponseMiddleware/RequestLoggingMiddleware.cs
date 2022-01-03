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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using NodaTime;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    public class RequestLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IRequestResponseLogging _requestResponseLogging;

        public RequestLoggingMiddleware(IRequestResponseLogging requestResponseLogging)
        {
            _requestResponseLogging = requestResponseLogging;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestContext = context.GetHttpRequestData();

            using var reader = new StreamReader(
                requestContext.Body,
                Encoding.UTF8,
                false,
                512,
                true);

            var metaData = BuildRequestLogInformation(context);
            var logName = LogDataBuilder.BuildLogName(metaData);
            await _requestResponseLogging.LogRequestAsync(reader.BaseStream, metaData, logName);

            requestContext.Body.Position = 0;
            await next(context);
        }

        private static Dictionary<string, string> BuildRequestLogInformation(FunctionContext context)
        {
            var bindingsFeature = context.GetHttpRequestData();

            var metaData = context.BindingContext.BindingData.ToDictionary(e => e.Key, pair => pair.Value as string ?? string.Empty);
            if (bindingsFeature is { } requestData)
            {
                foreach (var (key, value) in LogDataBuilder.ReadHeaderDataFromCollection(requestData.Headers))
                {
                    metaData.TryAdd(key, value);
                }

                metaData.TryAdd("FunctionId", context.FunctionId);
                metaData.TryAdd("InvocationId", context.InvocationId);
                metaData.TryAdd("TraceParent", context.TraceContext?.TraceParent ?? string.Empty);

                return metaData;
            }

            return metaData;
        }
    }
}
