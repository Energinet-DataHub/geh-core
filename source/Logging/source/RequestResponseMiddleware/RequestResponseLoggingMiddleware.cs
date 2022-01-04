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
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.Core.Logging.RequestResponseMiddleware
{
    public class RequestResponseLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IRequestResponseLogging _requestResponseLogging;

        public RequestResponseLoggingMiddleware(IRequestResponseLogging requestResponseLogging)
        {
            _requestResponseLogging = requestResponseLogging;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var requestLog = BuildRequestLogInformation(context);
            var requestLogName = LogDataBuilder.BuildLogName(requestLog.MetaData) + " request";
            await _requestResponseLogging.LogRequestAsync(requestLog.LogStream, requestLog.MetaData, requestLog.IndexTags, requestLogName).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            var responseLog = BuildResponseLogInformation(context);
            var responseLogName = LogDataBuilder.BuildLogName(requestLog.MetaData) + " response";
            await _requestResponseLogging.LogResponseAsync(responseLog.LogStream, responseLog.MetaData, responseLog.IndexTags, responseLogName).ConfigureAwait(false);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData, Dictionary<string, string> IndexTags) BuildRequestLogInformation(FunctionContext context)
        {
            var bindingsFeature = context.GetHttpRequestData();

            var metaData = context.BindingContext.BindingData
                .ToDictionary(e => LogDataBuilder.MetaNameFormatter(e.Key), pair => pair.Value as string ?? string.Empty);

            var indexTags =
                new Dictionary<string, string>(metaData.Where(e => e.Key != "headers" && e.Key != "query").Take(10));

            if (bindingsFeature is { } requestData)
            {
                foreach (var (key, value) in LogDataBuilder.ReadHeaderDataFromCollection(requestData.Headers))
                {
                    metaData.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
                }

                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionId"), context.FunctionId);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionName"), context.FunctionDefinition.Name);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("InvocationId"), context.InvocationId);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("TraceContext"), context.TraceContext?.TraceParent ?? string.Empty);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("HttpDataType"), "request");

                return (requestData.Body, metaData, indexTags);
            }

            return (Stream.Null, metaData, indexTags);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData, Dictionary<string, string> IndexTags) BuildResponseLogInformation(FunctionContext context)
        {
            var metaData = context.BindingContext.BindingData
                .ToDictionary(e => LogDataBuilder.MetaNameFormatter(e.Key), pair => pair.Value as string ?? string.Empty);

            var indexTags =
                new Dictionary<string, string>(metaData.Where(e => e.Key != "headers" && e.Key != "query").Take(10));

            if (context.GetHttpResponseData() is { } responseData)
            {
                foreach (var (key, value) in LogDataBuilder.ReadHeaderDataFromCollection(responseData.Headers))
                {
                    metaData.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
                    indexTags.TryAdd(LogDataBuilder.MetaNameFormatter(key), value);
                }

                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("StatusCode"), responseData.StatusCode.ToString());
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionId"), context.FunctionId);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionName"), context.FunctionDefinition.Name);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("InvocationId"), context.InvocationId);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("TraceContext"), context.TraceContext?.TraceParent ?? string.Empty);
                metaData.TryAdd(LogDataBuilder.MetaNameFormatter("HttpDataType"), "response");

                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("StatusCode"), responseData.StatusCode.ToString());
                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionId"), context.FunctionId);
                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("FunctionName"), context.FunctionDefinition.Name);
                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("InvocationId"), context.InvocationId);
                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("TraceContext"), context.TraceContext?.TraceParent ?? string.Empty);
                indexTags.TryAdd(LogDataBuilder.MetaNameFormatter("HttpDataType"), "response");

                return (responseData.Body, metaData, indexTags);
            }

            return (Stream.Null, metaData, indexTags);
        }
    }
}
