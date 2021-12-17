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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using NodaTime;

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
            var requestLogName = BuildLogName(requestLog.MetaData) + " response";
            await _requestResponseLogging.LogRequestAsync(requestLog.LogStream, requestLog.MetaData, requestLogName).ConfigureAwait(false);

            await next(context).ConfigureAwait(false);

            var responseLog = BuildResponseLogInformation(context);
            var responseLogName = BuildLogName(requestLog.MetaData) + " response";
            await _requestResponseLogging.LogResponseAsync(responseLog.LogStream, responseLog.MetaData, responseLogName).ConfigureAwait(false);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData) BuildRequestLogInformation(FunctionContext context)
        {
            var bindingsFeature = context.GetHttpRequestData();

            var metaData = context.BindingContext.BindingData.ToDictionary(e => e.Key, pair => pair.Value as string ?? string.Empty);
            if (bindingsFeature is { } requestData)
            {
                foreach (var (key, value) in ReadHeaderDataFromCollection(requestData.Headers))
                {
                    metaData.TryAdd(key, value);
                }

                metaData.TryAdd("FunctionId", context.FunctionId);
                metaData.TryAdd("InvocationId", context.InvocationId);
                metaData.TryAdd("TraceParent", context.TraceContext?.TraceParent ?? string.Empty);

                // TODO Should we "reset" stream ?
                return new ValueTuple<Stream, Dictionary<string, string>>(requestData.Body, metaData);
            }

            return new ValueTuple<Stream, Dictionary<string, string>>(Stream.Null, metaData);
        }

        private static (Stream LogStream, Dictionary<string, string> MetaData) BuildResponseLogInformation(FunctionContext context)
        {
            var metaData = context.BindingContext.BindingData.ToDictionary(e => e.Key, pair => pair.Value as string ?? string.Empty);
            if (context.GetHttpResponseData() is { } responseData)
            {
                foreach (var (key, value) in ReadHeaderDataFromCollection(responseData.Headers))
                {
                    metaData.TryAdd(key, value);
                }

                metaData.TryAdd("StatusCode", responseData.StatusCode.ToString());
                metaData.TryAdd("FunctionId", context.FunctionId);
                metaData.TryAdd("InvocationId", context.InvocationId);
                metaData.TryAdd("TraceParent", context.TraceContext?.TraceParent ?? string.Empty);

                // TODO Should we "reset" stream ?
                return new ValueTuple<Stream, Dictionary<string, string>>(responseData.Body, metaData);
            }

            return new ValueTuple<Stream, Dictionary<string, string>>(Stream.Null, metaData);
        }

        private static Dictionary<string, string> ReadHeaderDataFromCollection(HttpHeadersCollection headersCollection)
        {
            if (headersCollection is null)
            {
                return new Dictionary<string, string>();
            }

            var metaData = headersCollection
                .ToDictionary(e => e.Key, e => string.Join(",", e.Value));

            return metaData;
        }

        private static string BuildLogName(Dictionary<string, string> metaData)
        {
            metaData.TryGetValue("marketOperator", out var marketOperator);
            metaData.TryGetValue("recipient", out var recipient);
            metaData.TryGetValue("gln", out var gln);
            metaData.TryGetValue("glnNumber", out var glnNumber);
            metaData.TryGetValue("InvocationId", out var invocationId);
            metaData.TryGetValue("TraceParent", out var traceParent);
            metaData.TryGetValue("CorrelationId", out var correlationId);
            metaData.TryGetValue("FunctionId", out var functionId);

            var time = SystemClock.Instance.GetCurrentInstant().ToString();
            string name = $"{marketOperator ?? string.Empty}-" +
                          $"{recipient ?? string.Empty}-" +
                          $"{gln ?? string.Empty}-" +
                          $"{glnNumber ?? string.Empty}-" +
                          $"{invocationId ?? string.Empty}-" +
                          $"{traceParent ?? string.Empty}-" +
                          $"{correlationId ?? string.Empty}-" +
                          $"{functionId ?? string.Empty}-" +
                          $"{time}";
            return name.Replace("--", "-");
        }
    }
}
