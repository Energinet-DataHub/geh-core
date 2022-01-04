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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using NodaTime;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    public class ResponseLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IRequestResponseLogging _requestResponseLogging;

        public ResponseLoggingMiddleware(IRequestResponseLogging requestResponseLogging)
        {
            _requestResponseLogging = requestResponseLogging;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            await next(context);

            var contextResponse = context.GetHttpResponseData();
            await using var memoryStream = new MemoryStream();

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(contextResponse.Body);

            var logMetaData = BuildResponseLogInformation(context);
            var indexTags = new Dictionary<string, string>() { { "testIndex", "1" } };
            var logName = LogDataBuilder.BuildLogName(logMetaData) + " response";
            memoryStream.Position = 0;
            await _requestResponseLogging.LogResponseAsync(memoryStream, logMetaData, indexTags, logName);

            contextResponse.Body.Position = 0;
        }

        private static Dictionary<string, string> BuildResponseLogInformation(FunctionContext context)
        {
            var metaData = context.BindingContext.BindingData
                .ToDictionary(e => LogDataBuilder.MetaNameFormatter(e.Key), pair => pair.Value as string ?? string.Empty);

            if (context.GetHttpResponseData() is { } responseData)
            {
                foreach (var (key, value) in LogDataBuilder.ReadHeaderDataFromCollection(responseData.Headers))
                {
                    metaData.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
                }

                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("StatusCode"), responseData.StatusCode.ToString());
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionId"), context.FunctionId);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("InvocationId"), context.InvocationId);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("TraceParent"), context.TraceContext?.TraceParent ?? string.Empty);

                return metaData;
            }

            return metaData;
        }
    }
}
